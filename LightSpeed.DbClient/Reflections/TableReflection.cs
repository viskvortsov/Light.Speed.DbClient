using System.Reflection;
using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.Exceptions;

namespace LightSpeed.DbClient.Reflections;

public class TableReflection : ITableReflection
{

    private readonly string _name;
    private readonly string _queryName;
    private readonly Type _type;
    
    private readonly bool _isTranslatable;
    private readonly string _translationsTableName;
    
    private readonly List<IColumnReflection> _columns;
    private readonly List<IColumnReflection> _additionalFields = new();
    private readonly List<IColumnReflection> _additionalFields2 = new();
    private readonly List<IColumnReflection> _translatableColumns = new();
    private readonly List<IColumnReflection> _translatableColumns2 = new();
    private readonly List<IColumnReflection> _connectedTables;

    public TableReflection(Type type, String queryName)
    {
        
        _columns = new List<IColumnReflection>();
        _connectedTables = new List<IColumnReflection>();
        _type = type;
        _name = type.Name.ToLower();
        _queryName = queryName;
        
        TranslatableTableAttribute? translatable = _type.GetCustomAttribute<TranslatableTableAttribute>();
        _isTranslatable = translatable != null;
        if (_isTranslatable)
        {
            _translationsTableName = translatable!.Table;
        }
        
        FillColumns();
        FillTables();
        FillAdditionalFields();
        FillTranslatableColumns();

    }
    
    public TableReflection(Type type)
    {
        
        _columns = new List<IColumnReflection>();
        _connectedTables = new List<IColumnReflection>();
        _type = type;
        _name = type.Name.ToLower();
        
        ModelAttribute? model = _type.GetCustomAttribute<ModelAttribute>();
        if (model == null)
            throw new ClassIsNotAModelException($"Model not found for {_type.Name}");
        
        TranslatableTableAttribute? translatable = _type.GetCustomAttribute<TranslatableTableAttribute>();
        _isTranslatable = translatable != null;
        if (_isTranslatable)
        {
            _translationsTableName = translatable!.Table;
        }

        _queryName = model.Table;

        FillColumns();
        FillTables();
        FillAdditionalFields();
        FillTranslatableColumns();
        FillAdditionalTranslatableColumns();

    }
    
    private void FillColumns()
    {

        PropertyInfo[] properties = _type.GetProperties();
        foreach (var property in properties)
        {
            ColumnAttribute? column = property.GetCustomAttribute<ColumnAttribute>();
            if (column != null)
            {
                ColumnReflection columnReflection = new(property, this);
                if (_columns.Contains(columnReflection))
                {
                    throw new ReflectionException($"Model {_type.Name} has multiple columns with the same name {columnReflection.Name()}");
                }
                _columns.Add(columnReflection);
            }
        }
        
    }
    
    private void FillAdditionalFields()
    {

        PropertyInfo[] properties = _type.GetProperties();
        foreach (var property in properties)
        {
            AddInfoAttribute? column = property.GetCustomAttribute<AddInfoAttribute>();
            if (column != null)
            {
                ColumnReflection columnReflection = new(property, this);
                if (_additionalFields.Contains(columnReflection))
                {
                    throw new ReflectionException($"Model {_type.Name} has multiple columns with the same name {columnReflection.Name()}");
                }
                _additionalFields.Add(columnReflection);
            }
        }
        
        foreach (var property in properties)
        {
            AddInfoAdditionalAttribute? column = property.GetCustomAttribute<AddInfoAdditionalAttribute>();
            if (column != null)
            {
                ColumnReflection columnReflection = new(property, this);
                if (_additionalFields2.Contains(columnReflection))
                {
                    throw new ReflectionException($"Model {_type.Name} has multiple columns with the same name {columnReflection.Name()}");
                }
                _additionalFields2.Add(columnReflection);
            }
        }
        
    }
    
    private void FillTables()
    {

        PropertyInfo[] properties = _type.GetProperties();
        foreach (var property in properties)
        {
            TableAttribute? table = property.GetCustomAttribute<TableAttribute>();
            if (table != null)
            {
                ColumnReflection columnReflection = new(property, this);
                if (_connectedTables.Contains(columnReflection))
                {
                    throw new ReflectionException($"Model {_type.Name} has multiple tables with the same name {columnReflection.Name()}");
                }
                _connectedTables.Add(columnReflection);
            }
            TranslationsTableAttribute? translationsTable = property.GetCustomAttribute<TranslationsTableAttribute>();
            if (translationsTable != null)
            {
                ColumnReflection columnReflection = new(property, this);
                if (_connectedTables.Contains(columnReflection))
                {
                    throw new ReflectionException($"Model {_type.Name} has multiple tables with the same name {columnReflection.Name()}");
                }
                _connectedTables.Add(columnReflection);
            }
        }
        
    }
    
    private void FillTranslatableColumns()
    {
        foreach (var column in _columns)
        {
            if (column.IsTranslatable())
            {
                _translatableColumns.Add(column);
            }
        }
        foreach (var column in _additionalFields)
        {
            if (column.IsTranslatable())
            {
                _translatableColumns.Add(column);
            }
        }
    }
    
    private void FillAdditionalTranslatableColumns()
    {
        foreach (var column in _additionalFields2)
        {
            if (column.IsTranslatable())
            {
                _translatableColumns2.Add(column);
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

    public string TranslationsTableName()
    {
        return _translationsTableName;
    }

    public Type Type()
    {
        return _type;
    }

    public bool IsTranslatable()
    {
        return _isTranslatable;
    }

    public IEnumerable<IColumnReflection> Columns()
    {
        return _columns;
    }

    public string? TranslationsTableQueryName()
    {
        return _translationsTableName;
    }

    public IEnumerable<IColumnReflection> TranslatableColumns()
    {
        return _translatableColumns;
    }

    public IEnumerable<IColumnReflection> AdditionalTranslatableColumns()
    {
        return _translatableColumns2;
    }

    public IEnumerable<IColumnReflection> PartsOfPrimaryKey()
    {
        
        List<IColumnReflection> partsOfPrimaryKey = new();

        foreach (var column in _columns)
        {
            if (column.IsPartOfPrimaryKey())
                partsOfPrimaryKey.Add(column);
        }

        return partsOfPrimaryKey;

    }
    
    public IEnumerable<IColumnReflection> PartsOfOwnerKey()
    {
        
        List<IColumnReflection> partsOfOwnerKey = new();

        foreach (var column in _columns)
        {
            if (column.IsPartOfOwnerKey())
                partsOfOwnerKey.Add(column);
        }

        return partsOfOwnerKey;

    }

    public IColumnReflection GetColumnReflection(string name)
    {
        return _columns.Single(x => x.Name() == name.ToLower());
    }

    public IColumnReflection? GetColumnReflectionByQueryName(string name)
    {
        return _columns.Single(x => x.QueryName() == name.ToLower());
    }
    
    public IColumnReflection? GetAdditionalColumnReflection(string name)
    {
        return _additionalFields.Single(x => x.Name() == name.ToLower());
    }

    public IColumnReflection GetTableReflection(string name)
    {
        return _connectedTables.Single(x => x.Name() == name.ToLower());
    }

    public IEnumerable<IColumnReflection> ColumnsWithForeignKey()
    {
        List<IColumnReflection> columns = new ();
        foreach (var column in _columns)
        {
            if (column.HasForeignKeyTable())
                columns.Add(column);
        }
        return columns;
    }
    
    public IEnumerable<IColumnReflection> AdditionalFieldsWithForeignKey()
    {
        List<IColumnReflection> columns = new ();
        foreach (var column in _additionalFields)
        {
            if (column.HasAdditionalForeignKeyTable())
                columns.Add(column);
        }
        return columns;
    }

    public IEnumerable<IColumnReflection> ColumnsWithAdditionalInfo(string? foreignKeyName)
    {
        List<IColumnReflection> columns = new ();
        foreach (var column in _additionalFields)
        {
            if (column.ForeignKeyName() == foreignKeyName)
                columns.Add(column);
        }
        return columns;
    }
    
    public IEnumerable<IColumnReflection> ColumnsWithAdditionalInfo2(string? foreignKeyName)
    {
        List<IColumnReflection> columns = new ();
        foreach (var column in _additionalFields2)
        {
            if (column.ForeignKeyName() == foreignKeyName)
                columns.Add(column);
        }
        return columns;
    }

    public IEnumerable<IColumnReflection> AdditionalFields()
    {
        var columnsWithForeignKey = ColumnsWithForeignKey();
        List<IColumnReflection> allAdditionalFields = new ();
        var withForeignKey = columnsWithForeignKey.ToList();
        foreach (var column in withForeignKey)
        {
            foreach (IColumnReflection additionalField in ColumnsWithAdditionalInfo(column.ForeignKeyName()))
            {
                allAdditionalFields.Add(additionalField);
            }
        }

        return allAdditionalFields;
    }
    
    public IEnumerable<IColumnReflection> AdditionalFields2()
    {
        var columnsWithForeignKey = AdditionalFieldsWithForeignKey();
        List<IColumnReflection> allAdditionalFields = new ();
        var withForeignKey = columnsWithForeignKey.ToList();
        foreach (var column in withForeignKey)
        {
            foreach (IColumnReflection additionalField in ColumnsWithAdditionalInfo2(column.AdditionalForeignKeyName()))
            {
                allAdditionalFields.Add(additionalField);
            }
        }

        return allAdditionalFields;
    }

    public IColumnReflection GetForeignKeyColumn(string name)
    {

        IColumnReflection? foreignKeyColumn = null;
        foreach (var column in _columns)
        {
            if (column.HasForeignKeyTable() && column.ForeignKeyName().ToLower() == name.ToLower())
            {
                foreignKeyColumn = column;
                break;
            }
        }
        
        if (foreignKeyColumn == null)
            throw new ReflectionException($"Foreign key {name} not found for {_type.Name}");

        return foreignKeyColumn;

    }

    public IColumnReflection GetAdditionalForeignKeyColumn(string name)
    {
        IColumnReflection? foreignKeyColumn = null;
        foreach (var column in _additionalFields)
        {
            if (column.HasAdditionalForeignKeyTable() && column.AdditionalForeignKeyName().ToLower() == name.ToLower())
            {
                foreignKeyColumn = column;
                break;
            }
        }
        
        if (foreignKeyColumn == null)
            throw new ReflectionException($"Foreign key {name} not found for {_type.Name}");

        return foreignKeyColumn;
    }
}