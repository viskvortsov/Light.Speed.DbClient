using LightSpeed.DbClient.Exceptions;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;
using Npgsql;
using NpgsqlTypes;

namespace LightSpeed.DbClient.Postgresql.Database;

public static class PostgresqlDefaultSettings
{
    
    internal static readonly Dictionary<Type, string> DefaultColumnTypes = new()
    {
        { typeof(Guid),    "UUID" },
        { typeof(string),  "TEXT" },
        { typeof(bool),    "BOOLEAN" },
        { typeof(byte),    "BYTEA" },
        { typeof(DateTime),    "TIMESTAMP WITH TIME ZONE" },
        { typeof(decimal),    "NUMERIC" },
        { typeof(double),    "DOUBLE PRECISION" },
        { typeof(float),    "REAL" },
        { typeof(short),    "SMALLINT" },
        { typeof(int),    "INTEGER" },
        { typeof(long),    "BIGINT" },
        { typeof(Guid?),    "UUID" },
        { typeof(bool?),    "BOOLEAN" },
        { typeof(byte ?),    "BYTEA" },
        { typeof(DateTime?),    "TIMESTAMP WITH TIME ZONE" },
        { typeof(decimal ?),    "NUMERIC" },
        { typeof(double?),    "DOUBLE PRECISION" },
        { typeof(float ?),    "REAL" },
        { typeof(short?),    "SMALLINT" },
        { typeof(int?),    "INTEGER" },
        { typeof(long?),    "BIGINT" }
    };
    
    public static NpgsqlDbType GetSqlDbType(Type type, object value)
    {
        if (!PostgresqlMapper.DefaultTypes.TryGetValue(type, out NpgsqlDbType sqlType))
        {
            if (type.IsEnum)
            {
                sqlType = NpgsqlDbType.Integer;
            } 
            else if (type == typeof(ITranslatable))
            {
                sqlType = NpgsqlDbType.Uuid;
            }
            else if (type == typeof(TranslationRow.TranslationKey))
            {
                if (value is Guid)
                {
                    sqlType = NpgsqlDbType.Uuid;
                }
                else if (value is int)
                {
                    sqlType = NpgsqlDbType.Integer;
                }
                else
                {
                    throw new ReflectionException($"Sql type not found for type {type.FullName}"); 
                }
            }
            else
            {
                throw new TypeMappingException($"Sql type not found for type {type.FullName}");
            }
        }
        return sqlType; 
    }
    
    public static string GetSqlDbTypeName(Type type)
    {
        if (DefaultColumnTypes.TryGetValue(type, out string name))
            return name;
        if (type.IsEnum)
            return "int";
        if (type == typeof(ITranslatable) || type.GetInterface(typeof(ITranslatable).FullName!) != null)
            return "UUID";
        throw new TypeMappingException($"Sql type not found for type {type.FullName}");
    }
    
    public static object ConvertValue(object? value)
    {
        if (value == null)
            return DBNull.Value;
        if (value.GetType().IsEnum)
            return (int) value;
        if (value.GetType() == typeof(ITranslatable) || value.GetType().GetInterface(typeof(ITranslatable).FullName!) != null)
            return ((ITranslatable) value).GetId();
        return value;
    }
    
}