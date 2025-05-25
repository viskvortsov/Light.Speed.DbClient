using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;
using Npgsql;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlMapper(ITableReflection reflection, NpgsqlDataReader reader) : IMapper
{
    public IDatabaseObject MapToModel(IDatabaseObject element)
    {
        
        int i = 0;
        foreach (IColumnReflection column in reflection.Columns())
        {
            var value = MapToValue(i, column.Type());
            var property = column.Property();
            property.SetValue(element, value);
            i += 1;
        }

        // additional fields
        var additionalFields = reflection.AdditionalFields().ToList();
        
        foreach (var column in additionalFields)
        {
            var value = MapToValue(i, column.Type());
            var property = column.Property();
            property.SetValue(element, value);
            i += 1;
            // TODO where do we store it
        }
        
        return element;

    }
    
    public IDatabaseObjectTableElement MapToModel(IDatabaseObjectTableElement element)
    {
        
        int i = 0;
        foreach (IColumnReflection column in reflection.Columns())
        {
            var value = MapToValue(i, column.Type());
            var property = column.Property();
            property.SetValue(element, value);
            i += 1;
        }

        return element;

    }

    private object MapToValue(int index, Type type)
    {
        if (PostgresqlDefaultSettings.TypeReaders.TryGetValue(type, out var func))
        {
            return func(reader, index);
        }
        throw new NotSupportedException($"Type {type} is not supported.");
    }
    
}