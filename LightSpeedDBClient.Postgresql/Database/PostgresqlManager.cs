
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;
using Npgsql;
using NpgsqlTypes;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlManager<E> : Manager<E> where E : IDatabaseElement
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
            E element = MapToModel(reader);
            elements.Add(element);
        }

        return elements;
        
    }

    public override async Task<IEnumerable<E>> GetListAsync(int? page = null, int? limit = null)
    {
        return await GetListAsync(new List<IFilter>(), page, limit);
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
                receivedElement = MapToModel(reader);
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
                savedElement = MapToModel(reader);
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
    
    private E MapToModel(NpgsqlDataReader reader)
    {
        
        E element = Create();
        int i = 0;
        foreach (IColumnReflection column in Reflection.MainTableReflection.Columns())
        {
            var value = MapToValue(reader, i, column.Type());
            var property = column.Property();
            property.SetValue(element, value);
            i += 1;
        }

        return element;

    }

    private object MapToValue(NpgsqlDataReader reader, int index, Type type)
    {
        if (PostgresqlDefaultSettings.TypeReaders.TryGetValue(type, out var func))
        {
            return func(reader, index);
        }
        throw new NotSupportedException($"Type {type} is not supported.");
    }
    
}