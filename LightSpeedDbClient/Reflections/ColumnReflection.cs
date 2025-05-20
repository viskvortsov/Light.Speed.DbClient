using System.Reflection;
using LightSpeedDbClient.Attributes;

namespace LightSpeedDbClient.Reflections;

public class ColumnReflection : IColumnReflection
{
    
    private readonly string _name;
    private readonly string _queryName;
    private readonly Type _type;
    private readonly PropertyInfo _property;
    private readonly bool _isPartOfPrimaryKey;
    
    internal ColumnReflection(PropertyInfo property, ColumnAttribute column)
    {
        
        PrimaryKeyAttribute? key = property.GetCustomAttribute<PrimaryKeyAttribute>();
        
        _isPartOfPrimaryKey = false;
        if (key != null)
            _isPartOfPrimaryKey = true;
        
        _name = property.Name;
        _queryName = property.Name;
        _type = property.PropertyType;
        _property = property;

    }

    public string Name()
    {
        return _name;
    }

    public string QueryName()
    {
        return _queryName;
    }

    public Type Type()
    {
        return _type;
    }

    public PropertyInfo Property()
    {
        return _property;
    }

    public bool IsPartOfPrimaryKey()
    {
        return _isPartOfPrimaryKey;
    }
}