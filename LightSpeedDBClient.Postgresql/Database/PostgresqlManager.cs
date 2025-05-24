
using System.Collections;
using System.Reflection;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Implementations;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlManager<E> : Manager<E> where E : IDatabaseObject
{
    
    public PostgresqlManager(IConnection connection) : base(connection){}
    
    public PostgresqlManager(PostgresqlConnection connection) : base(connection){}

    public PostgresqlManager(PostgresqlConnection connection, PostgresqlTransaction transaction) : base(connection, transaction){}

    public PostgresqlManager(IConnection connection, ITransaction transaction) : base(connection, transaction){}

    public override async Task<IEnumerable<E>> GetListAsync(IFilters<E> filters, int? page = null, int? limit = null)
    {
        
        var elements = new List<E>();
        
        PostgresqlSelectListQuery<E> selectListQuery = new PostgresqlSelectListQuery<E>(filters, Reflection, page, limit);

        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }

        await using PostgresqlCommand cmd = new PostgresqlCommand(selectListQuery, (PostgresqlConnection)Connection, transaction);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            E element = CreateReference();
            element = (E) new PostgresqlMapper(Reflection.MainTableReflection, reader).MapToModel(element);
            elements.Add(element);
        }

        return elements;
        
    }

    public override async Task<IEnumerable<E>> GetListAsync(int? page = null, int? limit = null)
    {
        return await GetListAsync(new Filters<E>(), page, limit);
    }

    public override async Task<IEnumerable<E>> GetListObjectsAsync(IFilters<E> filters, int? page = null, int? limit = null)
    {
        Dictionary<IKey, E> elements = new ();
        
        PostgresqlSelectListObjectsQuery<E> selectListQuery = new PostgresqlSelectListObjectsQuery<E>(filters, Reflection, page, limit);

        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }

        await using PostgresqlCommand cmd = new PostgresqlCommand(selectListQuery, (PostgresqlConnection)Connection, transaction);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            E element = CreateObject();
            element = (E) new PostgresqlMapper(Reflection.MainTableReflection, reader).MapToModel(element);
            elements.Add(element.Key(), element);
        }
        foreach (IConnectedTable connectedTable in Reflection.ConnectedTables())
        {
            if (await reader.NextResultAsync())
            {
                List<IDatabaseObjectTableElement> list = new ();
                while (await reader.ReadAsync())
                {
                    IDatabaseObjectTableElement row = (IDatabaseObjectTableElement) CreateRow(connectedTable.TableReflection().Type());
                    row = new PostgresqlMapper(connectedTable.TableReflection(), reader).MapToModel(row);
                    list.Add(row);
                    // TODO how to sort them to elements?
                }
                    
                foreach (var row in list)
                {
                        
                    IKey ownerKey = row.OwnerKey();
                    List<KeyElement> keyParts = new List<KeyElement>();
                    foreach (var ownerKeyPart in ownerKey.KeyElements())
                    {
                        keyParts.Add(new KeyElement(Reflection.GetColumnReflection(ownerKeyPart.Column().Relation()), ownerKeyPart.Value()));
                    }
                    IKey primaryKey = new Key(keyParts);
                        
                    elements.TryGetValue(primaryKey, out E? savedElement);
                    if (savedElement == null)
                    {
                        throw new DatabaseException($"Error saving element, No element found for owner key {ownerKey}");
                    }
                    savedElement.Table(connectedTable.Name()).Add(row);
                        
                }
                int i=0;
            }
            else
            {
                throw new DatabaseException($"Error getting element by key, No information for table {connectedTable.QueryName()}");
            }
        }

        return elements.Values;
    }
    
    public override async Task<IEnumerable<E>> GetListObjectsAsync(int? page = null, int? limit = null)
    {
        return await GetListObjectsAsync(new Filters<E>(), page, limit);
    }

    public override async Task<int> CountAsync()
    {
        return await CountAsync(new Filters<E>());
    }

    public override async Task<int> CountAsync(IFilters<E> filters)
    {
        throw new NotImplementedException(); // TODO Implement count
    }

    public override async Task<E> GetByKeyAsync(IKey key)
    {

        E receivedElement = default(E);
        
        PostgresqlSelectByKeyQuery selectByKeyQuery = new (Reflection, key);

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
                receivedElement = CreateObject();
                receivedElement = (E) new PostgresqlMapper(Reflection.MainTableReflection, reader).MapToModel(receivedElement);
            }

            foreach (IConnectedTable connectedTable in Reflection.ConnectedTables())
            {
                if (await reader.NextResultAsync())
                {
                    List<IDatabaseObjectTableElement> list = new ();
                    while (await reader.ReadAsync())
                    {
                        IDatabaseObjectTableElement row = (IDatabaseObjectTableElement) CreateRow(connectedTable.TableReflection().Type());
                        row = new PostgresqlMapper(connectedTable.TableReflection(), reader).MapToModel(row);
                        list.Add(row);
                    }
                    convertToTable(connectedTable.Property(), receivedElement, list);
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

        return receivedElement;
        
    }
    
    public override async Task<E> SaveAsync(E element)
    {
        
        // TODO Check that all elements are objects
   
        PostgresqlSaveQuery<E> saveQuery = new (Reflection, element);

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

    public override async Task<IEnumerable<E>> SaveManyAsync(IEnumerable<E> elements, int chunkSize = 1000)
    {
        
        // TODO Check that all elements are objects
        // TODO create a 100k limit
        
        Dictionary<IKey, E> savedElements = new ();
        
        IEnumerable<E[]> chunks = elements.Chunk(chunkSize);
        
        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }
        
        PostgresqlBatch batch = new PostgresqlBatch((PostgresqlConnection)Connection, transaction);
        foreach (var chunk in chunks)
        {
            PostgresqlSaveManyQueries<E> saveQuery = new (Reflection, chunk);
            foreach (var subQuery in saveQuery.GetQueries())
            {
                PostgresqlBatchCommand cmd = new PostgresqlBatchCommand(subQuery);
                batch.AddCommand(cmd);
            }
        }
        
        try
        {
            
            await batch.ExecuteNonQueryAsync();
            
            Dictionary<string, Filter<E>> filtersDict = new Dictionary<string, Filter<E>>(); // TODO key should not be string
            foreach (var element in elements)
            {
                foreach (var keyElement in element.Key().KeyElements()) 
                {
                    filtersDict.TryGetValue(keyElement.Column().Name(), out Filter<E>? filter);
                    if (filter == null)
                    {
                        Type listType = typeof(List<>).MakeGenericType(keyElement.Column().Type());
                        IList list = (IList) Activator.CreateInstance(listType);
                        list.Add(keyElement.Value());
                        filter = new Filter<E>(keyElement.Column(), ComparisonOperator.In, list);  
                    }
                    else
                    {
                        filtersDict.Remove(keyElement.Column().Name());
                        IList list = (IList) filter.Value();
                        list.Add(keyElement.Value());
                        filter =  new Filter<E>(keyElement.Column(), ComparisonOperator.In, list);
                    }
                    filtersDict.Add(keyElement.Column().Name(), filter);
                }
            }

            Filters<E> filters = new ();
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
        return await DeleteAsync(new Filters<E>()); 
    }
    
    public override async Task<int> DeleteAsync(IFilters<E> filters)
    {
        PostgresqlDeleteListQuery<E> selectListQuery = new PostgresqlDeleteListQuery<E>(filters, Reflection);
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

    private void convertToTable(PropertyInfo property, E element, List<IDatabaseObjectTableElement> list)
    {
        
        Type listType = typeof(List<IDatabaseObjectTableElement>);
        ConstructorInfo constructor = property.PropertyType.GetConstructor(new Type[] { listType }); // TODO move to cache
        property.SetValue(element, constructor.Invoke(new object[] { list }));
        
    }
    
}