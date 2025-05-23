using LightSpeedDbClient.Database;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Implementations;

public class DatabaseObjectTableElement : IDatabaseObjectTableElement
{
    
    private readonly ITableReflection _reflection;
    
    public DatabaseObjectTableElement()
    {
        _reflection = ClientSettings.GetConnectedTableReflection(GetType());
    }
    
    public DatabaseObjectTableElement(ModelType modelType) : this()
    {
    }

    public IKey Key()
    {
        throw new NotImplementedException();
    }

    public IKey OwnerKey()
    {
        IEnumerable<IColumnReflection> partsOfKey = _reflection.PartsOfOwnerKey();
        List<KeyElement> keyElements = new List<KeyElement>();
        foreach (IColumnReflection keyElement in partsOfKey)
        {
            keyElements.Add(new KeyElement(keyElement, keyElement.Property().GetValue(this)));
        }
        return new Key(keyElements);
    }

    public ModelType ModelType()
    {
        return Models.ModelType.Row;
    }

    public bool IsObject()
    {
        return false;
    }

    public bool IsReference()
    {
        return false;
    }

    public bool IsRow()
    {
        return true;
    }
}