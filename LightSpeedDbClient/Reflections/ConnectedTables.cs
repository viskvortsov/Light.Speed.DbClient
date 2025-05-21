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
        
        PropertyInfo[] properties = type.GetProperties();
        foreach (var property in properties)
        {
            TableAttribute? table = property.GetCustomAttribute<TableAttribute>();
            if (table != null)
            {
                TableReflection tableReflection = new TableReflection(table.GetType());
                if (_tables.ContainsKey(tableReflection.Name()))
                {
                    throw new ReflectionException();
                }
                _tables.Add(tableReflection.Name(), tableReflection);
            }
        }
        
        
    }
    
}