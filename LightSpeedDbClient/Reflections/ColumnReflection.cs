using System.Reflection;
using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Reflections;

public class ColumnReflection : IColumnReflection
{
    
    private readonly string _name;
    private readonly string _queryName;
    private readonly Type _type;
    private readonly IQueryType _queryType;
    private readonly PropertyInfo _property;
    private readonly bool _isPartOfPrimaryKey;
    private readonly bool _isPartOfOwnerKey;
    private readonly string _relation;
    private readonly List<IColumnReflection> _additionalFields;
    private readonly ITableReflection _foreignKeyTable;
    private readonly IColumnReflection _foreignKeyColumn;
    
    internal ColumnReflection(PropertyInfo property)
    {
        
        _name = property.Name.ToLower();
        _type = property.PropertyType;
        _property = property;
        _additionalFields = new List<IColumnReflection>();
        
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

        if (column != null)
        {
            _queryType = ClientSettings.GetQueryType(_type);
            
            AddInfoAttribute? additionalInfo = property.GetCustomAttribute<AddInfoAttribute>();
            ForeignKeyAttribute? foreignKey = property.GetCustomAttribute<ForeignKeyAttribute>();
            if (additionalInfo != null && foreignKey != null)
            {
                Type type = foreignKey.Model;
                DatabaseObjectReflection reflection = ClientSettings.GetReflection(type);
                _foreignKeyTable = reflection.MainTableReflection;
                _foreignKeyColumn = reflection.MainTableReflection.GetColumnReflection(foreignKey.ColumnName);
                foreach (string additionalField in additionalInfo._fields)
                {
                    IColumnReflection additionalColumn = reflection.GetColumnReflection(additionalField);
                    if (additionalColumn == null)
                    {
                        throw new ReflectionException(); // TODO
                    } 
                    _additionalFields.Add(additionalColumn);
                }
            }
            
        }

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

    public IQueryType QueryType()
    {
        return _queryType;
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

    public bool HasAdditionalFields()
    {
        return _additionalFields.Count > 0;
    }

    public bool HasForeignKeyTable()
    {
        return _foreignKeyTable != null;
    }

    public IEnumerable<IColumnReflection> AdditionalFields()
    {
        return _additionalFields;
    }

    public ITableReflection ForeignKeyTable()
    {
        return _foreignKeyTable;
    }

    public IColumnReflection ForeignKeyColumn()
    {
        return _foreignKeyColumn;
    }
}