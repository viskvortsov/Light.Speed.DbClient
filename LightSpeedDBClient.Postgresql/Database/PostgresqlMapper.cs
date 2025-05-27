using System.Collections;
using System.Reflection;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Implementations;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;
using NpgsqlTypes;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlMapper(ITableReflection reflection) : IMapper
{
    
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
    
    public IDatabaseObject MapFromDatabaseToModel(IDatabaseObject element, List<object?> values)
    {
        int i = 0;
        
        foreach (IColumnReflection column in reflection.Columns())
        {
            var valueFromDb = values[i];
            var value = MapFromDatabaseValue(valueFromDb, column.Type());
            var property = column.Property();
            try
            {
                property.SetValue(element, value);
            }
            catch (TargetException ex)
            {
                throw new DatabaseException($"Error getting element by key", ex);
            }
            i++;
        }

        // additional fields
        var additionalFields = reflection.AdditionalFields().ToList();
        
        foreach (var column in additionalFields)
        {
            var valueFromDb = values[i];
            var value = MapFromDatabaseValue(valueFromDb, column.Type());
            var property = column.Property();
            try
            {
                property.SetValue(element, value);
            }
            catch (TargetException ex)
            {
                throw new DatabaseException($"Error getting element by key", ex);
            }
            i++;
        }
        
        return element;
    }

    public IDatabaseObjectTableElement MapFromDatabaseToModel(ITableReflection connectedTableReflection, IDatabaseObjectTableElement element, List<object?> values)
    {
        int i = 0;
        foreach (IColumnReflection column in connectedTableReflection.Columns())
        {
            var valueFromDb = values[i];
            var type = column.Type();
            var value = MapFromDatabaseValue(valueFromDb, type);
            var property = column.Property();
            try
            {
                property.SetValue(element, value);
            }
            catch (TargetException ex)
            {
                throw new DatabaseException($"Error getting element by key", ex);
            }
            i++;
        }
        // additional fields
        var additionalFields = connectedTableReflection.AdditionalFields().ToList();
        foreach (var column in additionalFields)
        {
            var valueFromDb = values[i];
            var value = MapFromDatabaseValue(valueFromDb, column.Type());
            var property = column.Property();
            try
            {
                property.SetValue(element, value);
            }
            catch (TargetException ex)
            {
                throw new DatabaseException($"Error getting element by key", ex);
            }
            i++;
        }
        return element;
    }

    public IDatabaseObject MapFromModelToDatabase(IDatabaseObject element)
    {
        throw new NotImplementedException();
    }

    public IDatabaseObjectTableElement MapFromModelToDatabase(ITableReflection connectedTableReflection, IDatabaseObjectTableElement element)
    {
        throw new NotImplementedException();
    }

    public object MapToDatabaseValue(object? value, Type type)
    {
        if (value == null)
            return DBNull.Value;

        if (DefaultTypes.ContainsKey(type))
        {
            return value;
        }
        else if (value.GetType() == typeof(ICollection) || value.GetType().GetInterface(typeof(ICollection).FullName!) != null)
        {
            ICollection list = (value as ICollection)!;
            object?[] values = new object?[list.Count];
            int i = 0;
            foreach (var item in list)
            {
                values[i] = MapToDatabaseValue(item, type);
                i++;
            }
            return values;
        }
        else if (type.IsEnum)
        {
            return (int)value;
        }
        else if (type == typeof(ITranslatable) || type.GetInterface(typeof(ITranslatable).FullName!) != null)
        {
            return ((ITranslatable) value).GetId(); 
        }
        else if (type == typeof(TranslationRow.TranslationKey))
        {
            TranslationRow.TranslationKey key = (TranslationRow.TranslationKey) value!;
            if (key.IsGuid)
            {
                return key.GuidId;
            }
            else if (key.IsInt)
            {
                return key.IntId;
            }
            else
            {
                throw new ReflectionException("Cannot convert value of type " + value.GetType() + " to type " + type); 
            }
        }
        else
        {
           throw new ReflectionException("Cannot convert value of type " + value.GetType() + " to type " + type); // TODO not reflection exception 
        }
    }

    public object? MapFromDatabaseValue(object? value, Type type)
    {
        if (value == null)
            return null;
        
        if (DefaultTypes.ContainsKey(type))
        {
            return Convert.ChangeType(value, type);
        }
        else if (type.IsEnum)
        {
            return Enum.ToObject(type, value);
        } else if (type == typeof(ITranslatable) || type.GetInterface(typeof(ITranslatable).FullName!) != null)
        {
            if (value == DBNull.Value)
                value = Guid.NewGuid();
            return new Translatable((Guid) value);
        }
        else if (type == typeof(TranslationRow.TranslationKey))
        {
            return new TranslationRow.TranslationKey(value);
        }
        else
        {
            throw new NotSupportedException($"Type {type} is not supported.");
        }
    }
}