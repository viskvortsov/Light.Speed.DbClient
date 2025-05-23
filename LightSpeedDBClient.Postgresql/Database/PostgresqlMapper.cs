using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;
using Npgsql;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlMapper : IMapper
{
    
    private readonly ITableReflection _tableReflection;
    private readonly NpgsqlDataReader _reader;

    public PostgresqlMapper(ITableReflection reflection, NpgsqlDataReader reader)
    {
        _tableReflection = reflection;
        _reader = reader;
    }

    public IDatabaseObject MapToModel(IDatabaseObject element)
    {
        
        int i = 0;
        foreach (IColumnReflection column in _tableReflection.Columns())
        {
            var value = MapToValue(_reader, i, column.Type());
            var property = column.Property();
            property.SetValue(element, value);
            i += 1;
        }

        return element;

    }
    
    public IDatabaseObjectTableElement MapToModel(IDatabaseObjectTableElement element)
    {
        
        int i = 0;
        foreach (IColumnReflection column in _tableReflection.Columns())
        {
            var value = MapToValue(_reader, i, column.Type());
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