using LightSpeedDbClient.Database;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Implementations;

public abstract class DatabaseObject : IDatabaseObject
{
    private readonly DatabaseObjectReflection _reflection;
    private readonly IKey _key;
    private readonly IObjectArrays _arrays;

    protected DatabaseObject(IKey key)
    {
        
        _key = key;
        _reflection = ClientSettings.GetReflection(GetType());

        IEnumerable<IColumnReflection> partsOfPrimaryKey = _reflection.MainTableReflection.PartsOfPrimaryKey();

        List<IKeyElement> keyElements = new List<IKeyElement>();
        foreach (IColumnReflection primaryKeyElement in partsOfPrimaryKey)
        {
            keyElements.Add(new KeyElement(primaryKeyElement, primaryKeyElement.Property().GetValue(this)));
        }
        
    }

    protected DatabaseObject() : this(new EmptyKey()) {}

    public IKey Key()
    {
        return _key;
    }

}