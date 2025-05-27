
using System.Collections;
using System.Reflection;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Implementations;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;
using Npgsql;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlManager<T> : Manager<T> where T : IDatabaseObject
{

    private static IMapper _mapper = new PostgresqlMapper(Reflection.MainTableReflection);
    
    public PostgresqlManager(IConnection connection) : base(connection){}
    
    public PostgresqlManager(PostgresqlConnection connection) : base(connection){}

    public PostgresqlManager(PostgresqlConnection connection, PostgresqlTransaction transaction) : base(connection, transaction){}

    public PostgresqlManager(IConnection connection, ITransaction transaction) : base(connection, transaction){}

    public override async Task<IEnumerable<T>> GetListAsync(IFilters<T> filters, int? page = null, int? limit = null)
    {
        return await GetListAsync(filters, new Sorting<T>(), page, limit);
    }

    public override async Task<IEnumerable<T>> GetListAsync(ISorting<T> sortBy, int? page = null, int? limit = null)
    {
        return await GetListAsync(new Filters<T>(), sortBy, page, limit);
    }

    public override async Task<IEnumerable<T>> GetListAsync(int? page = null, int? limit = null)
    {
        return await GetListAsync(new Filters<T>(), new Sorting<T>(), page, limit);
    }
    
    public override async Task<IEnumerable<T>> GetListAsync(IFilters<T> filters, ISorting<T> sortBy, int? page = null, int? limit = null)
    {
        var elements = new List<T>();
        
        PostgresqlSelectListQuery<T> selectListQuery = new PostgresqlSelectListQuery<T>(filters, Reflection, _mapper, page, limit);

        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }

        await using PostgresqlCommand cmd = new PostgresqlCommand(selectListQuery, (PostgresqlConnection)Connection, transaction);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            List<object?> values = GetAllValues(reader, Reflection.MainTableReflection);
            T element = CreateReference();
            element = (T) _mapper.MapFromDatabaseToModel(element, values);
            elements.Add(element);
        }
        foreach (var element in elements)
        {
            element.BeforeGetReference();
        }
        return elements;
    }

    public override async Task<IEnumerable<T>> GetListObjectsAsync(IFilters<T> filters, ISorting<T> sortBy, int? page = null, int? limit = null)
    {
        Dictionary<IKey, T> elements = new ();
        
        PostgresqlSelectListObjectsQuery<T> selectListQuery = new PostgresqlSelectListObjectsQuery<T>(filters, Reflection, _mapper, page, limit);

        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }

        await using PostgresqlCommand cmd = new PostgresqlCommand(selectListQuery, (PostgresqlConnection)Connection, transaction);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            List<object?> values = GetAllValues(reader, Reflection.MainTableReflection);
            T element = CreateObject();
            element = (T) _mapper.MapFromDatabaseToModel(element, values);
            elements.Add(element.Key(), element);
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

        foreach (var element in elements.Values)
        {
            element.BeforeGetObject();
        }
        return elements.Values;
    }
    
    public override async Task<IEnumerable<T>> GetListObjectsAsync(IFilters<T> filters, int? page = null, int? limit = null)
    {
        return await GetListObjectsAsync(new Filters<T>(), new Sorting<T>(), page, limit);
    }
    
    public override async Task<IEnumerable<T>> GetListObjectsAsync(ISorting<T> sortBy, int? page = null, int? limit = null)
    {
        return await GetListObjectsAsync(new Filters<T>(), sortBy, page, limit);
    }

    public override async Task<IEnumerable<T>> GetListObjectsAsync(int? page = null, int? limit = null)
    {
        return await GetListObjectsAsync(new Filters<T>(), new Sorting<T>(), page, limit); 
    }

    public override async Task<int> CountAsync()
    {
        return await CountAsync(new Filters<T>());
    }

    public override async Task<int> CountAsync(IFilters<T> filters)
    {
        throw new NotImplementedException(); // TODO Implement count
    }

    public override async Task<T> GetByKeyAsync(IKey key)
    {

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
                throw new DatabaseNotFoundException($"Error saving element");
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
        } 
        catch (Exception e)
        {
            throw new DatabaseException($"Error getting element by key", e);
        }
        
        if (receivedElement == null)
            throw new DatabaseNotFoundException($"Error saving element");

        receivedElement.BeforeGetObject();
        return receivedElement;
        
    }
    
    public override async Task<T> SaveAsync(T element)
    {
        
        // TODO Check that all elements are objects

        element.BeforeSave();
        PostgresqlSaveQuery<T> saveQuery = new (Reflection, element, _mapper);

        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }

        await using PostgresqlCommand cmd = new PostgresqlCommand(saveQuery, (PostgresqlConnection)Connection, transaction);

        try
        {
            await cmd.ExecuteNonQueryAsync();
        } 
        catch (Exception e)
        {
            throw new DatabaseSaveException($"Error saving element", e);
        }

        return await GetByKeyAsync(element.Key());
        
    }

    public override async Task<IEnumerable<T>> SaveManyAsync(IEnumerable<T> elements, int chunkSize = 1000)
    {
        
        // TODO Check that all elements are objects
        // TODO create a 100k limit

        foreach (var element in elements)
        {
            element.BeforeSave();
        }
        
        List<T> listOfElements = elements.ToList();
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
                                throw new ReflectionException($"Error creating list for type {keyElement.Column().Type().Name}"); // TODO not ReflectionException
                            list = (IList) createdList;
                        }
                        else
                        {
                            list = (IList) value;
                        }
                        if (list == null)
                            throw new ReflectionException($"Error creating list for type {keyElement.Column().Type().Name}"); // TODO not ReflectionException
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
        ConstructorInfo? constructor = property.PropertyType.GetConstructor([listType]); // TODO move to cache
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
        return values;
    }
    
}