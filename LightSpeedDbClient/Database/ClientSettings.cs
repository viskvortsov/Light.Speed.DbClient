using System.Reflection;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Database;

public static class ClientSettings
{
    
    private static Dictionary<Type, ConstructorInfo?> Constructors { get; } = new();
    private static Dictionary<Type, DatabaseObjectReflection> Reflections { get; } = new();
    private static Dictionary<Type, ITableReflection> ConnectedTablesReflections { get; } = new();
    private static Dictionary<Type, IQueryType> _queryTypes = new ();
    
    internal static ConstructorInfo? GetConstructor(Type type)
    {

        lock (Constructors)
        {
            
            if (Constructors.TryGetValue(type, out var constructor1))
            {
                return constructor1;
            }

            ConstructorInfo? info;

            ConstructorInfo? constructor = type.GetConstructor([typeof(ModelType)]);
            if (constructor == null)
                throw new ReflectionException($"Constructor not found for type {type.Name}");
            
            Constructors.Add(type, info = constructor);

            return info;

        }

    }
    
    internal static DatabaseObjectReflection GetReflection(Type type)
    {
        
        if (Reflections.TryGetValue(type, out var reflection1))
        {
            return reflection1;
        }

        lock (Reflections) lock(ConnectedTablesReflections)
        {
            
            if (Reflections.TryGetValue(type, out var reflection2))
            {
                return reflection2;
            }
            
            DatabaseObjectReflection reflection = new DatabaseObjectReflection(type);
            Reflections.Add(type, reflection);

            foreach (var connectedTable in reflection.ConnectedTables())
            {
                ConnectedTablesReflections.Add(connectedTable.TableReflection().Type(), connectedTable.TableReflection());
            }
            
            return reflection;

        }

    }
    
    internal static ITableReflection GetConnectedTableReflection(Type type)
    {
        
        if (ConnectedTablesReflections.TryGetValue(type, out var reflection))
        {
            return reflection;
        }

        throw new ReflectionException($"Connected table not found for type {type.Name}");

    }

    public static void SetQueryTypes(Dictionary<Type, IQueryType> queryTypes)
    {
        _queryTypes = queryTypes;
    }
    
    public static IQueryType GetQueryType(Type type)
    {
        _queryTypes.TryGetValue(type, out var value);
        if (value == null)
        {
            throw new ReflectionException($"Query type not found for type {type.Name}");
        }
        return value;
    }
    
}