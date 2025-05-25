using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Implementations;

public class GuidKey<T> : Key where T : IDatabaseElement
{
    
    public GuidKey(Guid id)
    {
        DatabaseObjectReflection reflection = ClientSettings.GetReflection(typeof(T));
        if (reflection.MainTableReflection.PartsOfPrimaryKey().Count() > 1)
            throw new ModelSetupException(
                "GuidKey can only be used with a single primary key column"
            );
        IColumnReflection column = reflection.MainTableReflection.PartsOfPrimaryKey().First();
        InternalKeyElements = new SortedList<string, KeyElement> { { column.Name(), new KeyElement(column, id) } };
    }
    
}