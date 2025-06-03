using System.Collections;
using System.Reflection;
using System.Text.Json;
using LightSpeed.DbClient.Exceptions;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Reflections;
using NpgsqlTypes;

namespace LightSpeed.DbClient.Postgresql.Database;

public class PostgresqlMapper(ITableReflection reflection) : IMapper
{
    
    internal static readonly Dictionary<Type, NpgsqlDbType> DefaultTypes = new()
    {
        { typeof(Guid),    NpgsqlDbType.Uuid },
        { typeof(string),  NpgsqlDbType.Text },
        { typeof(bool),    NpgsqlDbType.Boolean },
        { typeof(byte),    NpgsqlDbType.Bytea },
        { typeof(DateTime),    NpgsqlDbType.TimestampTz },
        { typeof(decimal),    NpgsqlDbType.Numeric },
        { typeof(double),    NpgsqlDbType.Double },
        { typeof(float),    NpgsqlDbType.Real },
        { typeof(short),    NpgsqlDbType.Smallint },
        { typeof(int),    NpgsqlDbType.Integer },
        { typeof(long),    NpgsqlDbType.Bigint },
        { typeof(Guid?),    NpgsqlDbType.Uuid },
        { typeof(bool?),    NpgsqlDbType.Boolean },
        { typeof(byte?),    NpgsqlDbType.Bytea },
        { typeof(DateTime?),    NpgsqlDbType.TimestampTz },
        { typeof(decimal?),    NpgsqlDbType.Numeric },
        { typeof(double?),    NpgsqlDbType.Double },
        { typeof(float?),    NpgsqlDbType.Real },
        { typeof(short?),    NpgsqlDbType.Smallint },
        { typeof(int?),    NpgsqlDbType.Integer },
        { typeof(long?),    NpgsqlDbType.Bigint }
    };
    
    public IDatabaseObject MapFromDatabaseToModel(IDatabaseObject element, List<object?> values)
    {
        int i = 0;
        
        foreach (IColumnReflection column in reflection.Columns())
        {
            var type = column.Type();
            var property = column.Property();
            
            try
            {
                var valueFromDb = values[i];
                var value = MapFromDatabaseValue(valueFromDb, type);
                property.SetValue(element, value);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error trying to set value to property {property.Name} for type {reflection.Name()}", ex);
            }
            i++;
        }

        // additional fields
        var additionalFields = reflection.AdditionalFields().ToList();
        
        foreach (var column in additionalFields)
        {
            var property = column.Property();
            var type = column.Type();
            
            try
            {
                var valueFromDb = values[i];
                var value = MapFromDatabaseValue(valueFromDb, type);
                property.SetValue(element, value);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error trying to set value to property {property.Name} for type {reflection.Name()}", ex);
            }
            i++;
        }
        
        // additional fields
        var additionalFields2 = reflection.AdditionalFields2().ToList();
        
        foreach (var column in additionalFields2)
        {
            var property = column.Property();
            var type = column.Type();
            
            try
            {
                var valueFromDb = values[i];
                var value = MapFromDatabaseValue(valueFromDb, type);
                property.SetValue(element, value);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error trying to set value to property {property.Name} for type {reflection.Name()}", ex);
            }
            i++;
        }
        
        var translatableFields = reflection.TranslatableColumns().ToList();
        foreach (var translatableField in translatableFields)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                continue;
            }

            var valueFromDb = values[i];
            var value = MapFromJson(valueFromDb);
            ITranslatable translatable = (ITranslatable)translatableField.Property().GetValue(element);
            foreach (var translation in value)
            {
                if (translation.language_id != null && translation.content != null)
                {
                    translatable!.AddTranslation((Guid)translation.language_id, translation.content);
                }
            }

            var property = translatableField.Property();
            try
            {
                property.SetValue(element, translatable);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(
                    $"Error trying to set value {value} to property {property.Name} for type {reflection.Name()}",
                    ex);
            }

            i++;
        }
        
        var translatableFields2 = reflection.AdditionalTranslatableColumns().ToList();
        foreach (var translatableField in translatableFields2)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                continue;
            }

            var valueFromDb = values[i];
            var value = MapFromJson(valueFromDb);
            ITranslatable translatable = (ITranslatable)translatableField.Property().GetValue(element);
            foreach (var translation in value)
            {
                if (translation.language_id != null && translation.content != null)
                {
                    translatable!.AddTranslation((Guid)translation.language_id, translation.content);
                }
            }

            var property = translatableField.Property();
            try
            {
                property.SetValue(element, translatable);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(
                    $"Error trying to set value {value} to property {property.Name} for type {reflection.Name()}",
                    ex);
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
            var type = column.Type();
            var property = column.Property();
            try
            {
                var valueFromDb = values[i];
                var value = MapFromDatabaseValue(valueFromDb, type);
                property.SetValue(element, value);
            }
            catch (Exception ex)
            {
                throw new DatabaseException(
                    $"Error trying to set value to property {property.Name} for type {connectedTableReflection.Name()}",
                    ex);
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
            catch (Exception ex)
            {
                throw new DatabaseException($"Error trying to set value {value} to property {property.Name} for type {connectedTableReflection.Name()}", ex);
            }
            i++;
        }
        // additional fields
        var additionalFields2 = connectedTableReflection.AdditionalFields2().ToList();
        foreach (var column in additionalFields2)
        {
            var valueFromDb = values[i];
            var value = MapFromDatabaseValue(valueFromDb, column.Type());
            var property = column.Property();
            try
            {
                property.SetValue(element, value);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error trying to set value {value} to property {property.Name} for type {connectedTableReflection.Name()}", ex);
            }
            i++;
        }
        var translatableFields = connectedTableReflection.TranslatableColumns().ToList();
        foreach (var translatableField in translatableFields)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                continue;
            }
            var valueFromDb = values[i];
            var value = MapFromJson(valueFromDb);
            ITranslatable translatable = (ITranslatable) translatableField.Property().GetValue(element);
            foreach (var translation in value)
            {
                if (translation.language_id != null && translation.content != null)
                {
                    translatable!.AddTranslation((Guid)translation.language_id, translation.content);
                }
            }
            var property = translatableField.Property();
            try
            {
                property.SetValue(element, translatable);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error trying to set value {value} to property {property.Name} for type {connectedTableReflection.Name()}", ex);
            }
            i++;
        }
        var translatableFields2 = connectedTableReflection.AdditionalTranslatableColumns().ToList();
        foreach (var translatableField in translatableFields2)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                continue;
            }
            var valueFromDb = values[i];
            var value = MapFromJson(valueFromDb);
            ITranslatable translatable = (ITranslatable) translatableField.Property().GetValue(element);
            foreach (var translation in value)
            {
                if (translation.language_id != null && translation.content != null)
                {
                    translatable!.AddTranslation((Guid)translation.language_id, translation.content);
                }
            }
            var property = translatableField.Property();
            try
            {
                property.SetValue(element, translatable);
            }
            catch (Exception ex)
            {
                throw new DatabaseException($"Error trying to set value {value} to property {property.Name} for type {connectedTableReflection.Name()}", ex);
            }
            i++;
        }
        return element;
    }

    private TranslationsJson[] MapFromJson(object? value)
    {
        return JsonSerializer.Deserialize<TranslationsJson[]>(value.ToString()!);
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
                throw new MappingException("Cannot convert value of type " + value.GetType() + " to type " + type); 
            }
        }
        else
        {
           throw new MappingException("Cannot convert value of type " + value.GetType() + " to type " + type); 
        }
    }

    public object? MapFromDatabaseValue(object? value, Type type)
    {
        try
        {
            if (value == null || value == DBNull.Value)
                return null;

            if (type == typeof(Guid) || type == typeof(Guid?))
            {
                return value;
            }
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return value;
            }
            else if (type == typeof(Boolean) || type == typeof(Boolean?))
            {
                return value;
            }
            else if (type == typeof(String))
            {
                return value;
            }
            else if (type == typeof(byte) || type == typeof(byte?))
            {
                return value;
            }
            else if (type == typeof(decimal) || type == typeof(decimal?))
            {
                return value;
            }
            else if (type == typeof(double) || type == typeof(double?))
            {
                return value;
            }
            else if (type == typeof(float) || type == typeof(float?))
            {
                return value;
            }
            else if (type == typeof(short) || type == typeof(short?))
            {
                return value;
            }
            else if (type == typeof(int) || type == typeof(int?))
            {
                return value;
            }
            else if (type == typeof(long) || type == typeof(long?))
            {
                return value;
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
        } catch (Exception ex)
        {
            throw new MappingException($"Error mapping value {value} to type {type}", ex);
        }
    }
}