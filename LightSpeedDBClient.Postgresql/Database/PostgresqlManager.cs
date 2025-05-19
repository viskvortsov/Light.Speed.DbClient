using System.Text;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;
using Npgsql;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlManager<E> : Manager<E> where E : IDatabaseElement
{
    
    private static readonly Dictionary<Type, Func<NpgsqlDataReader, int, object>> TypeReaders = new()
    {
        { typeof(Guid),    (reader, index) => reader.GetGuid(index) },
        { typeof(string),  (reader, index) => reader.GetString(index) },
        { typeof(bool),    (reader, index) => reader.GetBoolean(index) },
        { typeof(byte),    (reader, index) => reader.GetByte(index) },
        { typeof(DateTime),    (reader, index) => reader.GetDateTime(index) },
        { typeof(decimal),    (reader, index) => reader.GetDecimal(index) },
        { typeof(double),    (reader, index) => reader.GetDouble(index) },
        { typeof(float),    (reader, index) => reader.GetFloat(index) },
        { typeof(short),    (reader, index) => reader.GetInt16(index) },
        { typeof(int),    (reader, index) => reader.GetInt32(index) },
        { typeof(long),    (reader, index) => reader.GetInt64(index) }
    };
    
    public PostgresqlManager(IConnection connection) : base(connection){}
    
    public PostgresqlManager(PostgresqlConnection connection) : base(connection){}

    public PostgresqlManager(PostgresqlConnection connection, PostgresqlTransaction transaction) : base(connection, transaction){}

    public PostgresqlManager(IConnection connection, ITransaction transaction) : base(connection, transaction){}

    public override async Task<IEnumerable<E>> GetListAsync(int? page = null, int? limit = null)
    {
        
        var elements = new List<E>();
        
        PostgresqlSelectListQuery selectListQuery = new PostgresqlSelectListQuery(Reflection, page, limit);

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

    public override Task<int> CountAsync()
    {
        throw new NotImplementedException();
    }

    public override Task<E> GetAsync()
    {
        throw new NotImplementedException();
    }

    public override Task<E> SaveAsync(E element)
    {
        throw new NotImplementedException();
    }

    public override Task DeleteAsync()
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
        if (TypeReaders.TryGetValue(type, out var func))
        {
            return func(reader, index);
        }
        throw new NotSupportedException($"Type {type} is not supported.");
    }
    
}