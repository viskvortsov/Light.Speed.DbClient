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
    private readonly bool _isPartOfOwnerKey;
    private readonly string _relation;
    
    internal ColumnReflection(PropertyInfo property)
    {
        
        PrimaryKeyAttribute? primaryKey = property.GetCustomAttribute<PrimaryKeyAttribute>();
        
        _isPartOfPrimaryKey = false;
        if (primaryKey != null)
            _isPartOfPrimaryKey = true;
        
        OwnerKeyAttribute? ownerKey = property.GetCustomAttribute<OwnerKeyAttribute>();
        
        _isPartOfOwnerKey = false;
        if (ownerKey != null)
        {
            _isPartOfOwnerKey = true;
            _relation = ownerKey.Relation;
        }
        
        ColumnAttribute? column = property.GetCustomAttribute<ColumnAttribute>();
        
        _queryName = property.Name.ToLower();
        if (column != null && column.Name != null)
        {
            _queryName = column.Name.ToLower();
        }

        _name = property.Name.ToLower();
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

    public string Relation()
    {
        return _relation;
    }

    public PropertyInfo Property()
    {
        return _property;
    }

    public bool IsPartOfPrimaryKey()
    {
        return _isPartOfPrimaryKey;
    }

    public bool IsPartOfOwnerKey()
    {
        return _isPartOfOwnerKey;
    }
}