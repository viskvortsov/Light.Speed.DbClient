using System.Collections;
using System.Reflection;
using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Exceptions;

namespace LightSpeedDbClient.Reflections;

public class ConnectedTables : IConnectedTables
{
    
    private readonly Dictionary<string, ConnectedTable> _tables;

    public ConnectedTables(Type type)
    {
        
        ModelAttribute? model = type.GetCustomAttribute<ModelAttribute>();
        if (model == null)
            throw new ClassIsNotAModelException($"Model not found for type {type.Name}");

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
                    throw new ReflectionException($"Connected table {connectedTable.Name()} already exists");
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