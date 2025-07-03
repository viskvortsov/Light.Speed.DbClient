using System.Reflection;
using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.Database;
using LightSpeed.DbClient.Exceptions;
using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.Reflections;

public class ColumnReflection : IColumnReflection
{

    private readonly bool _isReadOnlyColumn;
    private readonly string _name;
    private readonly string _queryName;
    private readonly ITableReflection _tableReflection;
    private readonly Type _type;
    private readonly PropertyInfo _property;
    private readonly bool _isPartOfPrimaryKey;
    private readonly bool _isPartOfOwnerKey;
    private readonly bool _isTranslatable;
    private readonly string? _relation;
    private readonly IForeignKeyTable? _foreignKeyTable;
    private readonly string? _foreignKeyColumnName;
    private readonly Type? _foreignKeyType;
    private readonly string? _foreignKeyName;
    private readonly string? _addInfoColumnName;
    
    private readonly IForeignKeyTable? _additionalForeignKeyTable;
    private readonly string? _additionalForeignKeyColumnName;
    private readonly Type? _additionalForeignKeyType;
    private readonly string? _additionalForeignKeyName;
    
    internal ColumnReflection(PropertyInfo property, ITableReflection tableReflection)
    {
        
        _tableReflection = tableReflection;
        _name = property.Name.ToLower();
        _type = property.PropertyType;
        _property = property;
        _isTranslatable = _type == typeof(ITranslatable) || _type.GetInterfaces().Contains(typeof(ITranslatable));
        
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
                
                _foreignKeyType = foreignKey.Model;
                _foreignKeyTable = new ForeignKeyTable(_foreignKeyType);
                _foreignKeyName = foreignKey.Name;
                _foreignKeyColumnName = foreignKey.ColumnName;
            }
            
            _isReadOnlyColumn = column.ReadOnly;

            if (_isPartOfPrimaryKey && _isReadOnlyColumn)
            {
                throw new ReflectionException($"Column {_name} cannot be part of primary key and is read only at the same time");
            }
            
        }
        
        AddInfoAttribute? additionalInfo = property.GetCustomAttribute<AddInfoAttribute>();
        if (additionalInfo != null)
        {
            IColumnReflection foreignKeyColumn = _tableReflection.GetForeignKeyColumn(additionalInfo.ForeignKey);
            
            
            if (foreignKeyColumn == null)
                throw new ReflectionException($"Foreign key column {additionalInfo.ForeignKey} not found");
            
            if (additionalInfo.Field == null)
                throw new ReflectionException($"Field name not found in {additionalInfo.ForeignKey} key");
            
            _foreignKeyTable = foreignKeyColumn.ForeignKeyTableLink();
            _foreignKeyName = additionalInfo.ForeignKey;
            _foreignKeyType = foreignKeyColumn.Table().Type();
            _foreignKeyColumnName = foreignKeyColumn.QueryName();
            
            _queryName = additionalInfo.Field.ToLower();
            
            _addInfoColumnName = additionalInfo.Field;
            
            AddInfoForeignKeyAttribute? foreignKey = property.GetCustomAttribute<AddInfoForeignKeyAttribute>();
            if (foreignKey != null)
            {
                _additionalForeignKeyType = foreignKey.Model;
                _additionalForeignKeyTable = new ForeignKeyTable(_additionalForeignKeyType);
                _additionalForeignKeyName = foreignKey.Name;
                _additionalForeignKeyColumnName = foreignKey.ColumnName;
            }
            
        }
        
        AddInfoAdditionalAttribute? additionalInfo2 = property.GetCustomAttribute<AddInfoAdditionalAttribute>();
        if (additionalInfo2 != null)
        {
            IColumnReflection foreignKeyColumn = _tableReflection.GetAdditionalForeignKeyColumn(additionalInfo2.ForeignKey);

            if (foreignKeyColumn == null)
                throw new ReflectionException($"Foreign key column {additionalInfo2.ForeignKey} not found");
            
            if (additionalInfo2.Field == null)
                throw new ReflectionException($"Field name not found in {additionalInfo2.ForeignKey} key");
            
            _foreignKeyTable = foreignKeyColumn.ForeignKeyTableLink();
            _foreignKeyName = additionalInfo2.ForeignKey;
            _foreignKeyType = foreignKeyColumn.Table().Type();
            _foreignKeyColumnName = foreignKeyColumn.Name();
            
            _queryName = additionalInfo2.Field.ToLower();
            
            _addInfoColumnName = additionalInfo2.Field;
            
        }

    }

    public bool IsReadOnly()
    {
        return _isReadOnlyColumn;
    }

    public string Name()
    {
        return _name;
    }

    public string QueryName()
    {
        return _queryName;
    }

    public string TranslationsQueryName()
    {
        return $"{_queryName}_translations";
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
    
    public bool IsTranslatable()
    {
        return _isTranslatable;
    }

    public bool HasForeignKeyTable()
    {
        return _foreignKeyTable != null;
    }

    public bool HasAdditionalForeignKeyTable()
    {
        return _additionalForeignKeyTable != null;
    }

    public string ForeignKeyName()
    {
        return _foreignKeyName ?? throw new ReflectionException($"Foreign key name not found for column {_name}");
    }

    public string AdditionalForeignKeyName()
    {
        return _additionalForeignKeyName ?? throw new ReflectionException($"Foreign key name not found for column {_name}");
    }

    public ITableReflection ForeignKeyTable()
    {
        return _foreignKeyTable?.TableReflection() ?? throw new ReflectionException($"Foreign key table not found for column {_name}");
    }

    public ITableReflection AdditionalForeignKeyTable()
    {
        return _additionalForeignKeyTable?.TableReflection() ?? throw new ReflectionException($"Foreign key table not found for column {_name}");
    }

    public IForeignKeyTable ForeignKeyTableLink()
    {
        return _foreignKeyTable ?? throw new ReflectionException($"Foreign key table link not found for column {_name}");
    }

    public IColumnReflection ForeignKeyColumn()
    {
        return ClientSettings.GetReflection(_foreignKeyType).MainTableReflection.GetColumnReflection(_foreignKeyColumnName) ?? throw new ReflectionException($"Foreign key column not found for column {_name}");
    }
    
    public IColumnReflection AdditionalForeignKeyColumn()
    {
        return ClientSettings.GetReflection(_foreignKeyType).MainTableReflection.GetAdditionalColumnReflection(_foreignKeyColumnName) ?? throw new ReflectionException($"Foreign key column not found for column {_name}");
    }

    public string AdditionalForeignKeyColumnName()
    {
        return _additionalForeignKeyColumnName ?? throw new ReflectionException($"Foreign key column name not found for column {_name}");;
    }

    public ITableReflection Table()
    {
        return _tableReflection;
    }
    
}