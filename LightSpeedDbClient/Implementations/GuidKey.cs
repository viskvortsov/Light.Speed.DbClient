using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Implementations;

public class GuidKey<E> : Key where E : IDatabaseElement
{

    private readonly Guid _id;

    public GuidKey(Guid id)
    {
        DatabaseObjectReflection reflection = ClientSettings.GetReflection(typeof(E));
        if (reflection.MainTableReflection.PartsOfPrimaryKey().Count() > 1)
            throw new ModelSetupException(
                "GuidKey can only be used with a single primary key column"
            );
        IColumnReflection column = reflection.MainTableReflection.PartsOfPrimaryKey().First();
        _id = id;
        _keyElements = new List<KeyElement>();
        _keyElements.Add(new KeyElement(column, id));
    }
    
}