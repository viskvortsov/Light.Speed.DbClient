using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Implementations;

public class KeyElement : IKeyElement
{

    private readonly IColumnReflection _column;
    private readonly object _value;

    public KeyElement(IColumnReflection column, object value)
    {
        _column = column;
        _value = value;
    }

    public IColumnReflection Column()
    {
        return _column;
    }

    public object Value()
    {
        return _value;
    }
    
}