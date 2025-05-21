using System.Reflection;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Database;

public class ClientSettings
{
    
    private static Dictionary<Type, ConstructorInfo> _constructors { get; } = new();
    private static Dictionary<Type, DatabaseObjectReflection> _reflections { get; } = new();
    
    internal static ConstructorInfo GetConstructor(Type type)
    {

        lock (_constructors)
        {
            
            if (_constructors.ContainsKey(type))
            {
                return _constructors[type];
            }

            ConstructorInfo info;
            _constructors.Add(type, info = type.GetConstructor(new Type[] { }));

            return info;

        }

    }
    
    internal static DatabaseObjectReflection GetReflection(Type type)
    {
        
        if (_reflections.ContainsKey(type))
        {
            return _reflections[type];
        }

        lock (_reflections)
        {
            
            if (_reflections.ContainsKey(type))
            {
                return _reflections[type];
            }
            
            DatabaseObjectReflection reflection = new DatabaseObjectReflection(type);
            _reflections.Add(type, reflection);
            return reflection;

        }

    }
    
}