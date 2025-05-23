using System.Reflection;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Implementations;

public abstract class DatabaseObject : IDatabaseObject
{
    private readonly DatabaseObjectReflection _reflection;

    protected DatabaseObject()
    {
        _reflection = ClientSettings.GetReflection(GetType());
    }
    
    public IKey Key()
    {
        IEnumerable<IColumnReflection> partsOfPrimaryKey = _reflection.MainTableReflection.PartsOfPrimaryKey();
        List<KeyElement> keyElements = new List<KeyElement>();
        foreach (IColumnReflection primaryKeyElement in partsOfPrimaryKey)
        {
            keyElements.Add(new KeyElement(primaryKeyElement, primaryKeyElement.Property().GetValue(this)));
        }
        return new Key(keyElements);
    }

    public IDatabaseObjectTable Table(string name)
    {
        // TODO exception
        IColumnReflection columnReflection = _reflection.MainTableReflection.GetTableReflection(name);
        IDatabaseObjectTable table = (IDatabaseObjectTable) columnReflection.Property().GetValue(this);
        if (table == null)
        {
            ConstructorInfo constructor = ClientSettings.GetConstructor(columnReflection.Type());
            table = (IDatabaseObjectTable) constructor.Invoke(new object[]{});
            columnReflection.Property().SetValue(this, table);
        }
        return table;
    }

}