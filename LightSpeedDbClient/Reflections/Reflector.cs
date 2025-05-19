using System.Reflection;
using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Exceptions;

namespace LightSpeedDbClient.Reflections;

public class Reflector : IReflector
{
    
    private static readonly Dictionary<Type, ITableReflection> TableReflections = new();
    
    public ITableReflection GetTableReflection(Type type)
    {

        ITableReflection tableReflection;
        TableReflections.TryGetValue(type, out tableReflection);
        
        if (tableReflection == null)
        {
            lock (TableReflections)
            {
                TableReflections.TryGetValue(type, out tableReflection);
                if (tableReflection == null)
                {
                    tableReflection = CreateTableReflection(type);
                }
            }
        }

        return tableReflection!;

    }

    private ITableReflection CreateTableReflection(Type type)
    {
        
        ModelAttribute? modelAttribute = type.GetCustomAttribute<ModelAttribute>();
        if (modelAttribute == null)
        {
            throw new ClassIsNotAModelException();
        }
        
        return new TableReflection(type);

    }
    
}