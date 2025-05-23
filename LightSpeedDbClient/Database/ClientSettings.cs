using System.Reflection;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Database;

public class ClientSettings
{
    
    private static Dictionary<Type, ConstructorInfo> _constructors { get; } = new();
    private static Dictionary<Type, DatabaseObjectReflection> _reflections { get; } = new();
    private static Dictionary<Type, ITableReflection> _connectedTablesReflections { get; } = new();
    internal static ConstructorInfo GetConstructor(Type type)
    {

        lock (_constructors)
        {
            
            if (_constructors.ContainsKey(type))
            {
                return _constructors[type];
            }

            ConstructorInfo info;
            _constructors.Add(type, info = type.GetConstructor(new Type[] {}));

            return info;

        }

    }
    
    internal static DatabaseObjectReflection GetReflection(Type type)
    {
        
        if (_reflections.ContainsKey(type))
        {
            return _reflections[type];
        }

        lock (_reflections) lock(_connectedTablesReflections)
        {
            
            if (_reflections.ContainsKey(type))
            {
                return _reflections[type];
            }
            
            DatabaseObjectReflection reflection = new DatabaseObjectReflection(type);
            _reflections.Add(type, reflection);

            foreach (var connectedTable in reflection.ConnectedTables())
            {
                _connectedTablesReflections.Add(connectedTable.TableReflection().Type(), connectedTable.TableReflection());
            }
            
            return reflection;

        }

    }
    
    internal static ITableReflection GetConnectedTableReflection(Type type)
    {
        
        if (_connectedTablesReflections.ContainsKey(type))
        {
            return _connectedTablesReflections[type];
        }

        throw new ReflectionException(); // TODO

    }
    
}