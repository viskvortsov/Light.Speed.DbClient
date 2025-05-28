using System.Collections;
using System.Reflection;
using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.Exceptions;

namespace LightSpeed.DbClient.Reflections;

public class ConnectedTables : IConnectedTables
{
    
    private readonly Dictionary<string, IConnectedTable> _tables;

    public ConnectedTables(Type type)
    {
        ModelAttribute? model = type.GetCustomAttribute<ModelAttribute>();
        if (model == null)
            throw new ClassIsNotAModelException($"Model not found for type {type.Name}");

        _tables = new Dictionary<string, IConnectedTable>();
        
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