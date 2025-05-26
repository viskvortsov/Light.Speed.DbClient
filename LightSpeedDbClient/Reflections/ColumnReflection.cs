using System.Reflection;
using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;

namespace LightSpeedDbClient.Reflections;

public class ColumnReflection : IColumnReflection
{
    
    private readonly string _name;
    private readonly string _queryName;
    private readonly ITableReflection _tableReflection;
    private readonly Type _type;
    private readonly PropertyInfo _property;
    private readonly bool _isPartOfPrimaryKey;
    private readonly bool _isPartOfOwnerKey;
    
    private readonly List<IColumnReflection?> _additionalFields;
    private readonly string? _relation;
    private readonly ITableReflection? _foreignKeyTable;
    private readonly string? _foreignKeyName;
    private readonly IColumnReflection? _foreignKeyColumn;
    
    internal ColumnReflection(PropertyInfo property, ITableReflection tableReflection)
    {
        
        _tableReflection = tableReflection;
        _name = property.Name.ToLower();
        _type = property.PropertyType;
        _property = property;
        _additionalFields = new List<IColumnReflection?>();
        
        ColumnAttribute? column = property.GetCustomAttribute<ColumnAttribute>();
        
        _queryName = property.Name.ToLower();
        if (column?.Name != null)
        {
            _queryName = column.Name.ToLower();
        }

        if (column != null)
        {
            
            PrimaryKeyAttribute? primaryKey = property.GetCustomAttribute<PrimaryKeyAttribute>();
        
            _isPartOfPrimaryKey = primaryKey != null;

            OwnerKeyAttribute? ownerKey = property.GetCustomAttribute<OwnerKeyAttribute>();
        
            _isPartOfOwnerKey = false;
            if (ownerKey != null)
            {
                _isPartOfOwnerKey = true;
                _relation = ownerKey.Relation;
            }

            ForeignKeyAttribute? foreignKey = property.GetCustomAttribute<ForeignKeyAttribute>();
            if (foreignKey != null)
            {
                
                Type type = foreignKey.Model;
                DatabaseObjectReflection reflection = ClientSettings.GetReflection(type);
                _foreignKeyName = foreignKey.Name;
                _foreignKeyTable = reflection.MainTableReflection;
                _foreignKeyColumn = _foreignKeyTable.GetColumnReflection(foreignKey.ColumnName);
                
            }
            
        }
        
        AddInfoAttribute? additionalInfo = property.GetCustomAttribute<AddInfoAttribute>();
        if (additionalInfo != null)
        {
            IColumnReflection foreignKeyColumn = _tableReflection.GetForeignKeyColumn(additionalInfo.ForeignKey);

            ITableReflection? foreignKeyTable = foreignKeyColumn.ForeignKeyTable();
            if (foreignKeyTable == null)
                throw new ReflectionException($"Foreign key table {foreignKeyColumn.ForeignKeyName()} not found");
            
            DatabaseObjectReflection reflection = ClientSettings.GetReflection(foreignKeyTable.Type());
            
            if (additionalInfo.Field == null)
                throw new ReflectionException($"Field name not found in {additionalInfo.ForeignKey} key");
            
            _foreignKeyName = additionalInfo.ForeignKey;
            _queryName = additionalInfo.Field.ToLower();
            
            _foreignKeyTable = reflection.MainTableReflection;
            if (_foreignKeyTable == null)
                throw new ReflectionException($"Foreign key table {additionalInfo.ForeignKey} not found");
            
            _foreignKeyColumn = _foreignKeyTable.GetColumnReflection(additionalInfo.Field);
                
            IColumnReflection? additionalColumn = reflection.GetColumnReflection(additionalInfo.Field);
            if (additionalColumn == null)
            {
                throw new ReflectionException($"Column {additionalInfo.Field} not found in table {_foreignKeyTable.Name()}");
            } 
            _additionalFields.Add(additionalColumn);
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
    
    public string? Relation()
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

    public IEnumerable<IColumnReflection?> AdditionalFields()
    {
        return _additionalFields;
    }

    public string ForeignKeyName()
    {
        return _foreignKeyName ?? throw new ReflectionException($"Foreign key name not found for column {_name}");
    }

    public ITableReflection ForeignKeyTable()
    {
        return _foreignKeyTable ?? throw new ReflectionException($"Foreign key table not found for column {_name}");
    }

    public IColumnReflection ForeignKeyColumn()
    {
        return _foreignKeyColumn ?? throw new ReflectionException($"Foreign key column not found for column {_name}");
    }

    public ITableReflection Table()
    {
        return _tableReflection;
    }
    
}