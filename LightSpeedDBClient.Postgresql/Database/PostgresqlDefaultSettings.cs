using Npgsql;
using NpgsqlTypes;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlDefaultSettings
{
    
    internal static readonly Dictionary<Type, Func<NpgsqlDataReader, int, object>> TypeReaders = new()
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
    
    internal static readonly Dictionary<Type, NpgsqlDbType> DefaultTypes = new()
    {
        { typeof(Guid),    NpgsqlDbType.Uuid },
        { typeof(string),  NpgsqlDbType.Varchar },
        { typeof(bool),    NpgsqlDbType.Boolean },
        { typeof(byte),    NpgsqlDbType.Bytea },
        { typeof(DateTime),    NpgsqlDbType.TimestampTz },
        { typeof(decimal),    NpgsqlDbType.Numeric },
        { typeof(double),    NpgsqlDbType.Double },
        { typeof(float),    NpgsqlDbType.Real },
        { typeof(short),    NpgsqlDbType.Smallint },
        { typeof(int),    NpgsqlDbType.Integer },
        { typeof(long),    NpgsqlDbType.Bigint }
    };
    
}