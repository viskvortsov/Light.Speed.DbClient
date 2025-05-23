using System.Reflection;
using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Exceptions;

namespace LightSpeedDbClient.Reflections;

public class TableReflection : ITableReflection
{

    private readonly string _name;
    private readonly string _queryName;
    private readonly Type _type;
    
    private readonly List<IColumnReflection> _columns;
    private readonly List<IColumnReflection> _connectedTables;
    private readonly Dictionary<string, IColumnReflection> _partsOfPrimaryKey;
    private readonly Dictionary<string, IColumnReflection> _partsOfOwnerKey;
    
    public TableReflection(Type type)
    {
        
        _columns = new List<IColumnReflection>();
        _connectedTables = new List<IColumnReflection>();
        _partsOfPrimaryKey = new Dictionary<string, IColumnReflection>();
        _partsOfOwnerKey = new Dictionary<string, IColumnReflection>();
        _type = type;
        _name = type.Name.ToLower();
        
        ModelAttribute? model = _type.GetCustomAttribute<ModelAttribute>();
        if (model == null)
            throw new ClassIsNotAModelException();

        _queryName = model.Table;

        FillColumns();
        FillTables();
        FillPrimaryKeys();
        FillOwnerKeys();

    }
    
    private void FillColumns()
    {

        PropertyInfo[] properties = _type.GetProperties();
        foreach (var property in properties)
        {
            ColumnAttribute? column = property.GetCustomAttribute<ColumnAttribute>();
            if (column != null)
            {
                ColumnReflection columnReflection = new(property);
                if (_columns.Contains(columnReflection))
                {
                    throw new ReflectionException();
                }
                _columns.Add(columnReflection);
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
                ColumnReflection columnReflection = new(property);
                if (_connectedTables.Contains(columnReflection))
                {
                    throw new ReflectionException();
                }
                _connectedTables.Add(columnReflection);
            }
        }
        
    }
    
    private void FillPrimaryKeys()
    {
        
        foreach (var column in _columns)
        { 
            if (column.IsPartOfPrimaryKey())
                _partsOfPrimaryKey.Add(column.Name(), column);
        }
        
    }
    
    private void FillOwnerKeys()
    {
        
        foreach (var column in _columns)
        { 
            if (column.IsPartOfOwnerKey())
                _partsOfOwnerKey.Add(column.Name(), column);
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

    public IEnumerable<IColumnReflection> Columns()
    {
        return _columns;
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
        return _columns.SingleOrDefault(x => x.Name() == name.ToLower());
    }

    public IColumnReflection GetTableReflection(string name)
    {
        return _connectedTables.SingleOrDefault(x => x.Name() == name.ToLower());
    }

    public List<IColumnReflection> ColumnsWithAdditionalInfo()
    {
        List<IColumnReflection> columns = new ();
        foreach (var column in _columns)
        {
            if (column.HasForeignKeyTable() && column.HasAdditionalFields())
            {
                columns.Add(column);
            }
        }
        return columns;
    }
}