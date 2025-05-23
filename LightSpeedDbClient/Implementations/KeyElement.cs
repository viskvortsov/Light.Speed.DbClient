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
        bool columnNamesEqual = 
            (_column == null && other._column == null) ||
            (_column != null && other._column != null && 
             _column.Name().Equals(other._column.Name()));
        
        if (!columnNamesEqual) return false;
        
        // Compare values
        return (_value == null && other._value == null) ||
               (_value != null && _value.Equals(other._value));
    }
    
    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17; // Prime number to start with
        
            // Include the column's hash code if it's not null
            hash = hash * 31 + (_column != null ? _column.Name().GetHashCode() : 0);
        
            // Include the value's hash code if it's not null
            hash = hash * 31 + (_value != null ? _value.GetHashCode() : 0);
        
            return hash;
        }
    }
    
}