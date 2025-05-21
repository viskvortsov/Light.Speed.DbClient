using System.Collections;
using System.Reflection;
using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Exceptions;

namespace LightSpeedDbClient.Reflections;

public class ConnectedTables : IConnectedTables
{
    
    Dictionary<string, ITableReflection> _tables;

    public ConnectedTables(Type type)
    {
        
        ModelAttribute? model = type.GetCustomAttribute<ModelAttribute>();
        if (model == null)
            throw new ClassIsNotAModelException();

        _tables = new Dictionary<string, ITableReflection>();
        
        PropertyInfo[] properties = type.GetProperties();
        foreach (var property in properties)
        {
            TableAttribute? table = property.GetCustomAttribute<TableAttribute>();
            if (table != null)
            {
                Type rowType = property.PropertyType.GetInterface("IObjectTable`1").GetGenericArguments()[0];
                TableReflection tableReflection = new TableReflection(rowType);
                if (_tables.ContainsKey(tableReflection.Name()))
                {
                    throw new ReflectionException();
                }
                _tables.Add(tableReflection.Name(), tableReflection);
            }
        }
        
        
    }

    public IEnumerator<ITableReflection> GetEnumerator()
    {
        return _tables.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
}