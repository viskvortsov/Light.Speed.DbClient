using System.Collections;
using System.Reflection;
using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Exceptions;

namespace LightSpeedDbClient.Reflections;

public class ConnectedTables : IConnectedTables
{
    
    Dictionary<string, ConnectedTable> _tables;

    public ConnectedTables(Type type)
    {
        
        ModelAttribute? model = type.GetCustomAttribute<ModelAttribute>();
        if (model == null)
            throw new ClassIsNotAModelException();

        _tables = new Dictionary<string, ConnectedTable>();
        
        PropertyInfo[] properties = type.GetProperties();
        foreach (var property in properties)
        {
            TableAttribute? table = property.GetCustomAttribute<TableAttribute>();
            if (table != null)
            {
                ConnectedTable connectedTable = new ConnectedTable(property);
                if (_tables.ContainsKey(connectedTable.Name()))
                {
                    throw new ReflectionException();
                }
                _tables.Add(connectedTable.Name(), connectedTable);
            }
        }
        
        
    }

    public IEnumerator<IConnectedTable> GetEnumerator()
    {
        return _tables.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
}