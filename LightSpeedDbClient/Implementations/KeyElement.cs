using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Implementations;

public class KeyElement(IColumnReflection column, object? value) : IKeyElement
{

    private readonly IColumnReflection _column = column;
    private readonly object? _value = value;

    public IColumnReflection Column()
    {
        return _column;
    }

    public object? Value()
    {
        return _value;
    }

    public Type Type()
    {
        return _column.Type();
    }

    public override bool Equals(object? obj)
    {
        if (obj is KeyElement keyElement)
            return Equals(keyElement);
        return false;
    }
    
    public bool Equals(KeyElement? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        // Compare column names
        bool columnNamesEqual = (_column.Name().Equals(other._column.Name()));
        
        if (!columnNamesEqual) return false;
        
        // Compare values
        return (_value == null && other._value == null) ||
               (_value != null && _value.Equals(other._value));
    }
    
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _column.Name().GetHashCode();
            hash = hash * 31 + (_value != null ? _value.GetHashCode() : 0);
        
            return hash;
        }
    }
    
}