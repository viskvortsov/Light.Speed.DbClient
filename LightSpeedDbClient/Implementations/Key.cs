using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public class Key : IKey
{
    internal SortedList<string, KeyElement> _keyElements;
    
    public Key()
    {
    }
    public Key(List<KeyElement> elements)
    {
        _keyElements = new ();
        foreach (var keyElement in elements)
        {
            _keyElements.Add(keyElement.Column().Name(), keyElement);
        }
    }
    public IEnumerable<IKeyElement> KeyElements()
    {
        return _keyElements.Values.ToList();
    }

    public override bool Equals(object? obj)
    {
        // Check for null and reference equality
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
    
        var other = (Key)obj;
    
        // Handle null lists
        if (_keyElements == null && other._keyElements == null)
            return true;
    
        if (_keyElements == null || other._keyElements == null)
            return false;
    
        // Check if lists have the same number of elements
        if (_keyElements.Count != other._keyElements.Count)
            return false;
    
        // Compare all key-value pairs
        foreach (var key in _keyElements.Keys)
        {
            // Check if the key exists in the other collection
            if (!other._keyElements.ContainsKey(key))
                return false;
        
            // Check if the values are equal
            if (!Equals(_keyElements[key], other._keyElements[key]))
                return false;
        }
    
        return true;
    }

    public override int GetHashCode()
    {
        
        if (_keyElements == null)
            return 0;
    
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17; // Prime number to start with
        
            foreach (var element in _keyElements.Values)
            {
                hash = hash * 31 + (element != null ? element.GetHashCode() : 0);
            }
        
            return hash;
        }

    }
    
}