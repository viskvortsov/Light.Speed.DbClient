
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

    public override async Task<IEnumerable<E>> GetListAsync(IEnumerable<IFilter> filters, int? page = null, int? limit = null)
    {
        
        var elements = new List<E>();
        
        PostgresqlSelectListQuery selectListQuery = new PostgresqlSelectListQuery(filters, Reflection, page, limit);

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
        return await GetListAsync(new List<IFilter>(), page, limit);
    }

    public override async Task<IEnumerable<E>> GetListObjectsAsync(IEnumerable<IFilter> filters, int? page = null, int? limit = null)
    {
        Dictionary<IKey, E> elements = new ();
        
        PostgresqlSelectListObjectsQuery selectListQuery = new PostgresqlSelectListObjectsQuery(filters, Reflection, page, limit);

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
        return await GetListObjectsAsync(new List<IFilter>(), page, limit);
    }

    public override async Task<int> CountAsync()
    {
        return await CountAsync(new List<IFilter>());
    }

    public override async Task<int> CountAsync(IEnumerable<IFilter> filters)
    {
        throw new NotImplementedException();
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
        
        E? savedElement = default(E);
        
        PostgresqlSaveQuery<E> saveQuery = new (Reflection, element);

        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }

        await using PostgresqlCommand cmd = new PostgresqlCommand(saveQuery, (PostgresqlConnection)Connection, transaction);

        try
        {
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                savedElement = CreateObject();
                savedElement = (E) new PostgresqlMapper(Reflection.MainTableReflection, reader).MapToModel(savedElement);
            }
            foreach (IConnectedTable connectedTable in Reflection.ConnectedTables())
            {
                if (await reader.NextResultAsync())
                {
                    List<IDatabaseObjectTableElement> list = new ();
                    while (await reader.ReadAsync())
                    {
                        ITableReflection tableReflection = connectedTable.TableReflection();
                        Type tableReflectionType = tableReflection.Type();
                        IDatabaseObjectTableElement row = (IDatabaseObjectTableElement) CreateRow(tableReflectionType);
                        row = new PostgresqlMapper(connectedTable.TableReflection(), reader).MapToModel(row);
                        list.Add(row);
                    }
                    convertToTable(connectedTable.Property(), savedElement, list);
                }
                else
                {
                    throw new DatabaseException($"Error getting element by key, No information for table {connectedTable.QueryName()}");
                }
            }
        } 
        catch (Exception e)
        {
            throw new DatabaseSaveException($"Error saving element", e);
        }
        
        if (savedElement == null)
            throw new DatabaseSaveException($"Error saving element");

        return savedElement;
        
    }

    public override async Task<IEnumerable<E>> SaveManyAsync(IEnumerable<E> elements)
    {
        
        // TODO Check that all elements are objects
        
        Dictionary<IKey, E> savedElements = new ();
        
        PostgresqlSaveQuery<E> saveQuery = new (Reflection, elements);

        PostgresqlTransaction? transaction = null;
        if (Transaction != null)
        {
            transaction = (PostgresqlTransaction)Transaction;
        }

        await using PostgresqlCommand cmd = new PostgresqlCommand(saveQuery, (PostgresqlConnection)Connection, transaction);

        try
        {
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                E element = CreateObject();
                E savedElement = (E) new PostgresqlMapper(Reflection.MainTableReflection, reader).MapToModel(element);
                savedElements.Add(savedElement.Key(), savedElement);
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
                        
                        savedElements.TryGetValue(primaryKey, out E? savedElement);
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
        } 
        catch (Exception e)
        {
            throw new DatabaseSaveException($"Error saving element", e);
        }
        
        return savedElements.Values;
        
    }


    public override async Task<int> DeleteAsync()
    {
        return await DeleteAsync(new List<IFilter>()); 
    }
    
    public override async Task<int> DeleteAsync(IEnumerable<IFilter> filters)
    {
        PostgresqlDeleteListQuery selectListQuery = new PostgresqlDeleteListQuery(filters, Reflection);
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