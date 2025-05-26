
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Reflections;

public interface IMapper
{
    IDatabaseObject MapFromDatabaseToModel(IDatabaseObject element, List<object?> values);
    IDatabaseObjectTableElement MapFromDatabaseToModel(ITableReflection reflection, IDatabaseObjectTableElement element, List<object?> values);
    
    IDatabaseObject MapFromModelToDatabase(IDatabaseObject element);
    IDatabaseObjectTableElement MapFromModelToDatabase(ITableReflection reflection, IDatabaseObjectTableElement element);

    object? MapToDatabaseValue(object? value, Type type);
    object? MapFromDatabaseValue(object? value, Type type);
    
}