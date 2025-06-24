
using System.Collections;
using System.Reflection;
using LightSpeed.DbClient.Database;
using LightSpeed.DbClient.Exceptions;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Reflections;
using Npgsql;

namespace LightSpeed.DbClient.Postgresql.Database;

public class PostgresqlManager<T> : Manager<T> where T : IDatabaseObject
{

    private static IMapper _mapper = new PostgresqlMapper(Reflection.MainTableReflection);
    
    public PostgresqlManager(IConnection connection) : base(connection){}
    
    public PostgresqlManager(PostgresqlConnection connection) : base(connection){}

    public PostgresqlManager(PostgresqlConnection connection, PostgresqlTransaction transaction) : base(connection, transaction){}

    public PostgresqlManager(IConnection connection, ITransaction transaction) : base(connection, transaction){}

    public override async Task<IDataSelection<T>> GetListAsync(IFilters<T> filters, int? page = null, int? limit = null)
    {
        return await GetListAsync(filters, new Sorting<T>(), page, limit);
    }

    public override async Task<IDataSelection<T>> GetListAsync(ISorting<T> sortBy, int? page = null, int? limit = null)
    {
        return await GetListAsync(new Filters<T>(), sortBy, page, limit);
    }

    public override async Task<IDataSelection<T>> GetListAsync(int? page = null, int? limit = null)
    {
        return await GetListAsync(new Filters<T>(), new Sorting<T>(), page, limit);
    }
    
    public override async Task<IDataSelection<T>> GetListAsync(IFilters<T> filters, ISorting<T> sortBy, int? page = null, int? limit = null)
    {
        List<T> sortedElements = new List<T>();
        Dictionary<IKey, T> elements = new ();
        
        PostgresqlCountQuery<T> countQuery = new PostgresqlCountQuery<T>(filters, Reflection, _mapper);
        PostgresqlSelectListObjectsQuery<T> selectListQuery = new PostgresqlSelectListObjectsQuery<T>(filters, sortBy, ModelType.Reference, Reflection, _mapper, page, limit);

        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }

        await using PostgresqlCommand cmd1 = new PostgresqlCommand(countQuery, (PostgresqlConnection)Connection, transaction);
        await using var countReader = await cmd1.ExecuteReaderAsync();
        long count = 0;
        if (await countReader.ReadAsync())
        {
            // Value may be System.DBNull
            object value = countReader.GetValue(0);
            if (value is long)
                count = (long) value;
        }
        await countReader.CloseAsync();
        if (count == 0)
        {
            if (page != null && limit != null)
            {
                return new PaginatedDataSelection<T>((long)page!, (long)limit!, sortedElements, count);
            }
            else
            {
                return new DataSelection<T>(sortedElements);
            }
        }
        await using PostgresqlCommand cmd2 = new PostgresqlCommand(selectListQuery, (PostgresqlConnection)Connection, transaction);
        await using var reader = await cmd2.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            List<object?> values = GetAllValues(reader, Reflection.MainTableReflection);
            T element = CreateReference();
            element = (T) _mapper.MapFromDatabaseToModel(element, values);
            elements.Add(element.Key(), element);
            sortedElements.Add(element);
        }
        foreach (IConnectedTable connectedTable in Reflection.TranslationTables())
        {
            await ProcessConnectedTable(connectedTable, elements, reader);
        }
        await reader.CloseAsync();
        foreach (var element in elements.Values)
        {
            element.BeforeGetReference();
        }
        
        IDataSelection<T> finalElements = new DataSelection<T>(sortedElements);
        if (page != null && limit != null)
        {
            finalElements = new PaginatedDataSelection<T>((long)page!, (long)limit!, sortedElements, count);
        }
        
        return finalElements;
    }

    public override async Task<IDataSelection<T>> GetListObjectsAsync(IFilters<T> filters, ISorting<T> sortBy, int? page = null, int? limit = null)
    {
        List<T> sortedElements = new List<T>();
        Dictionary<IKey, T> elements = new ();
        
        PostgresqlCountQuery<T> countQuery = new PostgresqlCountQuery<T>(filters, Reflection, _mapper);
        PostgresqlSelectListObjectsQuery<T> selectListQuery = new PostgresqlSelectListObjectsQuery<T>(filters, sortBy, ModelType.Object, Reflection, _mapper, page, limit);

        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }
        await using PostgresqlCommand cmd1 = new PostgresqlCommand(countQuery, (PostgresqlConnection)Connection, transaction);
        await using var countReader = await cmd1.ExecuteReaderAsync();
        long count = 0;
        if (await countReader.ReadAsync())
        {
            // Value may be System.DBNull
            object value = countReader.GetValue(0);
            if (value is long)
                count = (long) value;
        }
        await countReader.CloseAsync();
        if (count == 0)
        {
            if (page != null && limit != null)
            {
                return new PaginatedDataSelection<T>((long)page!, (long)limit!, sortedElements, count);
            }
            else
            {
                return new DataSelection<T>(sortedElements);
            }
        }
        await using PostgresqlCommand cmd2 = new PostgresqlCommand(selectListQuery, (PostgresqlConnection)Connection, transaction);
        await using var reader = await cmd2.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            List<object?> values = GetAllValues(reader, Reflection.MainTableReflection);
            T element = CreateObject();
            element = (T) _mapper.MapFromDatabaseToModel(element, values);
            elements.Add(element.Key(), element);
            sortedElements.Add(element);
        }
        foreach (IConnectedTable connectedTable in Reflection.ConnectedTables())
        {
            await ProcessConnectedTable(connectedTable, elements, reader);
        }
        foreach (IConnectedTable connectedTable in Reflection.TranslationTables())
        {
            await ProcessConnectedTable(connectedTable, elements, reader);
        }
        await reader.CloseAsync();
        
        foreach (var element in elements.Values)
        {
            element.BeforeGetObject();
        }

        IDataSelection<T> finalElements = new DataSelection<T>(sortedElements);
        if (page != null && limit != null)
        {
            finalElements = new PaginatedDataSelection<T>((long)page!, (long)limit!, sortedElements, count);
        }
        
        return finalElements;
    }

    private async Task ProcessConnectedTable(IConnectedTable connectedTable, Dictionary<IKey, T> elements, NpgsqlDataReader reader)
    {
        if (await reader.NextResultAsync())
        {
            List<IDatabaseObjectTableElement> list = new ();
            while (await reader.ReadAsync())
            {
                ITableReflection connectedTableReflection = connectedTable.TableReflection();
                List<object?> values = GetAllValues(reader, connectedTableReflection);
                IDatabaseObjectTableElement row = (IDatabaseObjectTableElement) CreateRow(connectedTable.TableReflection().Type());
                row = _mapper.MapFromDatabaseToModel(connectedTable.TableReflection(), row, values);
                list.Add(row);
            }
                    
            foreach (var row in list)
            {
                        
                IKey ownerKey = row.OwnerKey();
                List<KeyElement> keyParts = new List<KeyElement>();
                foreach (var ownerKeyPart in ownerKey.KeyElements())
                {
                    string? relation = ownerKeyPart.Column().Relation();
                    if (relation == null)
                        throw new ModelSetupException($"Relation not found for owner key {ownerKeyPart.Column().Name()}");
                    keyParts.Add(new KeyElement(Reflection.GetColumnReflection(relation), _mapper.MapToDatabaseValue(ownerKeyPart.Value(), ownerKeyPart.Column().Type())));;
                }
                IKey primaryKey = new Key(keyParts);
                        
                elements.TryGetValue(primaryKey, out T? savedElement);
                if (savedElement == null)
                {
                    throw new DatabaseException($"Error saving element, No element found for owner key {ownerKey}");
                }
                savedElement.Table(connectedTable.Name()).Add(row);
            }
        }
        else
        {
            throw new DatabaseException($"Error getting element by key, No information for table {connectedTable.QueryName()}");
        }
    }
    
    public override async Task<IDataSelection<T>> GetListObjectsAsync(IFilters<T> filters, int? page = null, int? limit = null)
    {
        return await GetListObjectsAsync(filters, new Sorting<T>(), page, limit);
    }
    
    public override async Task<IDataSelection<T>> GetListObjectsAsync(ISorting<T> sortBy, int? page = null, int? limit = null)
    {
        return await GetListObjectsAsync(new Filters<T>(), sortBy, page, limit);
    }

    public override async Task<IDataSelection<T>> GetListObjectsAsync(int? page = null, int? limit = null)
    {
        return await GetListObjectsAsync(new Filters<T>(), new Sorting<T>(), page, limit); 
    }

    public override async Task<long> CountAsync()
    {
        return await CountAsync(new Filters<T>());
    }

    public override async Task<long> CountAsync(IFilters<T> filters)
    {
        PostgresqlCountQuery<T> selectListQuery = new PostgresqlCountQuery<T>(
            filters, 
            Reflection, 
            _mapper
        );

        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }
        await using PostgresqlCommand cmd = new PostgresqlCommand(selectListQuery, (PostgresqlConnection)Connection, transaction);
        await using var countReader = await cmd.ExecuteReaderAsync();
        long count = 0;
        if (await countReader.ReadAsync())
        {
            // Value may be System.DBNull
            object value = countReader.GetValue(0);
            if (value is long)
                count = (long) value;
        }
        await countReader.CloseAsync();
        return count;
    }

    public override async Task<T> GetByKeyAsync(IKey key)
    {
        
        if (!typeof(DatabaseObject).IsAssignableFrom(typeof(T)))
            throw new NotSupportedException("Only DatabaseObject types are supported");

        T? receivedElement = default(T);
        
        PostgresqlSelectByKeyQuery selectByKeyQuery = new (Reflection, key, _mapper);

        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }

        await using PostgresqlCommand cmd = new PostgresqlCommand(selectByKeyQuery, (PostgresqlConnection)Connection, transaction);

        try
        {
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                List<object?> values = GetAllValues(reader, Reflection.MainTableReflection);
                receivedElement = CreateObject();
                receivedElement = (T) _mapper.MapFromDatabaseToModel(receivedElement, values);
            }

            if (receivedElement == null)
            {
                throw new DatabaseNotFoundException($"Error getting element by key");
            }

            foreach (IConnectedTable connectedTable in Reflection.ConnectedTables())
            {
                if (await reader.NextResultAsync())
                {
                    List<IDatabaseObjectTableElement> list = new ();
                    while (await reader.ReadAsync())
                    {
                        List<object?> values = GetAllValues(reader, connectedTable.TableReflection());
                        IDatabaseObjectTableElement row = (IDatabaseObjectTableElement) CreateRow(connectedTable.TableReflection().Type());
                        row = _mapper.MapFromDatabaseToModel(connectedTable.TableReflection(), row, values);
                        list.Add(row);
                    }
                    ConvertToTable(connectedTable.Property(), receivedElement, list);
                }
                else
                {
                    throw new DatabaseException($"Error getting element by key, No information for table {connectedTable.QueryName()}");
                }
            }
            await reader.CloseAsync();
        } 
        catch (Exception e)
        {
            throw new DatabaseException($"Error getting element by key", e);
        }
        
        if (receivedElement == null)
            throw new DatabaseNotFoundException($"Error getting element by key");

        receivedElement.BeforeGetObject();
        return receivedElement;
        
    }
    
    public override async Task<T> SaveAsync(T element)
    {
        List<T> elements = new List<T> { element };
        IEnumerable<T> savedElements = await SaveManyAsync(elements);
        var databaseObjects = savedElements.ToList();
        if (databaseObjects.Count() == 1)
            return databaseObjects.First();
        throw new DatabaseException($"Error saving element");
    }

    public override async Task<IDataSelection<T>> SaveManyAsync(IEnumerable<T> elements, int chunkSize = 1000)
    {
        var databaseObjects = elements.ToList();
        if (databaseObjects.Count() > 100000)
        {
            throw new NotSupportedException("Saving more then 100k elements in one batch is not supported");
        }

        foreach (var element in databaseObjects)
        {
            if (!element.ModelType().Equals(ModelType.Object))
                throw new NotSupportedException($"Only {ModelType.Object} models are supported");
            element.BeforeSave();
        }
        
        List<T> listOfElements = databaseObjects.ToList();
        IEnumerable<T[]> chunks = listOfElements.Chunk(chunkSize);
        
        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }
        
        PostgresqlBatch batch = new PostgresqlBatch((PostgresqlConnection)Connection, transaction);
        foreach (var chunk in chunks)
        {
            PostgresqlSaveManyQueries<T> saveQuery = new (Reflection, chunk, _mapper);
            foreach (var subQuery in saveQuery.GetQueries())
            {
                PostgresqlBatchCommand cmd = new PostgresqlBatchCommand(subQuery);
                batch.AddCommand(cmd);
            }
        }
        
        try
        {
            
            await batch.ExecuteNonQueryAsync();
            
            Dictionary<string, Filter<T>> filtersDict = new Dictionary<string, Filter<T>>(); // TODO key should not be string
            foreach (var element in listOfElements)
            {
                foreach (var keyElement in element.Key().KeyElements()) 
                {
                    filtersDict.TryGetValue(keyElement.Column().Name(), out Filter<T>? filter);
                    if (filter == null)
                    {
                        Type listType = typeof(List<>).MakeGenericType(keyElement.Type());
                        object? value = Activator.CreateInstance(listType); // TODO possible performance issues
                        if (value == null)
                            throw new ReflectionException($"Error creating list for type {keyElement.Type().Name}");
                        IList list = (IList) value;
                        list.Add(keyElement.Value());
                        filter = new Filter<T>(keyElement.Column(), ComparisonOperator.In, list);  
                    }
                    else
                    {
                        filtersDict.Remove(keyElement.Column().Name());
                        object? value = filter.Value();
                        IList? list;
                        if (value == null)
                        {
                            Type listType = typeof(List<>).MakeGenericType(keyElement.Column().Type());
                            object? createdList = Activator.CreateInstance(listType);
                            if (createdList == null)
                                throw new MappingException($"Error creating list for type {keyElement.Column().Type().Name}");
                            list = (IList) createdList;
                        }
                        else
                        {
                            list = (IList) value;
                        }
                        if (list == null)
                            throw new MappingException($"Error creating list for type {keyElement.Column().Type().Name}");
                        list.Add(keyElement.Value());
                        filter =  new Filter<T>(keyElement.Column(), ComparisonOperator.In, list);
                    }
                    filtersDict.Add(keyElement.Column().Name(), filter);
                }
            }

            Filters<T> filters = new ();
            foreach (var keyValue in filtersDict)
            {
                filters.Add(keyValue.Value);
            }
        
            return await GetListObjectsAsync(filters);
            
        } 
        catch (Exception e)
        {
            throw new DatabaseSaveException($"Error saving element", e);
        }
        
    }

    public override async Task<T> SaveRecordsAsync(IFilters<T> filters, T element)
    {
        if (!typeof(RecordObject).IsAssignableFrom(typeof(T)))
            throw new NotSupportedException("Only RecordObject types are supported");
        await DeleteAsync(filters);
        return await SaveAsync(element);
    }

    public override async Task<IDataSelection<T>> SaveRecordsAsync(IFilters<T> filters, IEnumerable<T> elements, int chunkSize = 1000)
    {
        if (!typeof(RecordObject).IsAssignableFrom(typeof(T)))
            throw new NotSupportedException("Only RecordObject types are supported");
        await DeleteAsync(filters);
        return await SaveManyAsync(elements, chunkSize);
    }

    public override async Task<int> DeleteAsync()
    {
        return await DeleteAsync(new Filters<T>()); 
    }
    
    public override async Task<int> DeleteAsync(IFilters<T> filters)
    {
        PostgresqlDeleteListQuery<T> selectListQuery = new PostgresqlDeleteListQuery<T>(filters, Reflection, _mapper);
        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }
        await using PostgresqlCommand cmd = new PostgresqlCommand(selectListQuery, (PostgresqlConnection)Connection, transaction);
        return await cmd.ExecuteNonQueryAsync();
    }

    public override async Task<int> DeleteByKeyAsync(IKey key)
    {
        throw new NotImplementedException();
    }

    private void ConvertToTable(PropertyInfo property, T element, List<IDatabaseObjectTableElement> list)
    {
        Type listType = typeof(List<IDatabaseObjectTableElement>);
        ConstructorInfo? constructor = property.PropertyType.GetConstructor([listType]); // TODO performance issues?
        if (constructor == null)
            throw new ReflectionException($"Constructor not found for type {property.PropertyType.Name}");
        property.SetValue(element, constructor.Invoke([list]));
    }

    private List<object?> GetAllValues(NpgsqlDataReader reader, ITableReflection reflection)
    {
        List<object?> values = new();
        int i = 0;
        foreach (IColumnReflection column in reflection.Columns())
        {
            values.Add(reader.GetValue(i));
            i += 1;
        }
        foreach (IColumnReflection column in reflection.AdditionalFields())
        {
            values.Add(reader.GetValue(i));
            i += 1;
        }
        foreach (IColumnReflection column in reflection.AdditionalFields2())
        {
            values.Add(reader.GetValue(i));
            i += 1;
        }
        var translatableFields = reflection.TranslatableColumns().ToList();
        foreach (var translatableField in translatableFields)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                continue;
            }
            values.Add(reader.GetValue(i));
            i++;
        }
        var translatableFields2 = reflection.AdditionalTranslatableColumns().ToList();
        foreach (var translatableField in translatableFields2)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                continue;
            }
            values.Add(reader.GetValue(i));
            i++;
        }
        return values;
    }
    
}