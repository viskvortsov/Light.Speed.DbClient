using System.Reflection;
using LightSpeedDbClient.Attributes;

namespace LightSpeedDbClient.Reflections;

public class ConnectedTable : IConnectedTable
{
    
    private readonly string _name;
    private readonly string _queryName;
    private readonly Type _type;
    private readonly PropertyInfo _property;
    private readonly ITableReflection _tableReflection;

    public ConnectedTable(PropertyInfo property)
    {
        
        TableAttribute? table = property.GetCustomAttribute<TableAttribute>();
        
        _name = property.Name.ToLower();
        _type = property.PropertyType;
        _property = property;
        
        Type rowType = property.PropertyType.GetGenericArguments()[0];
        _tableReflection = new TableReflection(rowType);
        _queryName = _tableReflection.QueryName();

    }
    
    public string Name()
    {
        return _name;
    }

    public string QueryName()
    {
        return _queryName;
    }

    public ITableReflection TableReflection()
    {
        return _tableReflection;
    }

    public Type Type()
    {
        return _type;
    }

    public PropertyInfo Property()
    {
        return _property;
    }
}