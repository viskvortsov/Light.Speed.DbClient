using System.Collections;
using System.Reflection;
using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Reflections;

public class TableReflection : ITableReflection
{

    private readonly string _name;
    private readonly string _queryName;
    private readonly Type _type;
    
    private readonly Dictionary<string, IColumnReflection> _columns;
    private readonly Dictionary<string, IColumnReflection> _partsOfPrimaryKey;
    
    public TableReflection(Type type)
    {
        
        _columns = new Dictionary<string, IColumnReflection>();
        _partsOfPrimaryKey = new Dictionary<string, IColumnReflection>();
        _type = type;
        _name = type.Name.ToLower();
        
        ModelAttribute? model = _type.GetCustomAttribute<ModelAttribute>();
        if (model == null)
            throw new ClassIsNotAModelException();

        _queryName = model.Table;

        FillColumns();
        FillKeys();

    }
    
    private void FillColumns()
    {

        PropertyInfo[] properties = _type.GetProperties();
        foreach (var property in properties)
        {
            ColumnAttribute? column = property.GetCustomAttribute<ColumnAttribute>();
            if (column != null)
            {
                ColumnReflection columnReflection = new(property, column);
                if (_columns.ContainsKey(columnReflection.Name()))
                {
                    throw new ReflectionException();
                }
                _columns.Add(columnReflection.Name(), columnReflection);
            }
        }
        
    }
    
    private void FillKeys()
    {
        
        foreach (var column in _columns.Values)
        { 
            if (column.IsPartOfPrimaryKey())
                _partsOfPrimaryKey.Add(column.Name(), column);
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

    public IEnumerable<IColumnReflection> Columns()
    {
        return _columns.Values;
    }

    public IEnumerable<IColumnReflection> PartsOfPrimaryKey()
    {
        
        List<IColumnReflection> partsOfPrimaryKey = new();

        foreach (var column in _columns.Values)
        {
            if (column.IsPartOfPrimaryKey())
                partsOfPrimaryKey.Add(column);
        }

        return partsOfPrimaryKey;

    }
}