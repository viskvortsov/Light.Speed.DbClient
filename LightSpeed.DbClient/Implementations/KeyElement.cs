using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Reflections;

namespace LightSpeed.DbClient.Implementations;

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

        if (_value != null && other._value != null)
        {
            if (_value.GetType().IsEnum && other._value is int)
            {
                return ((int)_value).Equals(other._value);
            }

            if (_value.GetType() is int && other._value.GetType().IsEnum)
            {
                return _value.Equals((int)other._value);
            }
        }

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