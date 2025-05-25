using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public class Key : IKey
{
    internal SortedList<string, KeyElement> InternalKeyElements;

    protected Key()
    {
        InternalKeyElements = new ();
    }
    
    public Key(List<KeyElement> elements)
    {
        InternalKeyElements = new ();
        foreach (var keyElement in elements)
        {
            InternalKeyElements.Add(keyElement.Column().Name(), keyElement);
        }
    }
    public IEnumerable<IKeyElement> KeyElements()
    {
        return InternalKeyElements.Values.ToList();
    }

    public override bool Equals(object? obj)
    {
        // Check for null and reference equality
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
    
        var other = (Key)obj;
        
        // Check if lists have the same number of elements
        if (InternalKeyElements.Count != other.InternalKeyElements.Count)
            return false;
    
        // Compare all key-value pairs
        foreach (var key in InternalKeyElements.Keys)
        {
            // Check if the key exists in the other collection
            if (!other.InternalKeyElements.ContainsKey(key))
                return false;
        
            // Check if the values are equal
            if (!Equals(InternalKeyElements[key], other.InternalKeyElements[key]))
                return false;
        }
    
        return true;
    }

    public override int GetHashCode()
    {
        
        unchecked
        {
            int hash = 17;
        
            foreach (var element in InternalKeyElements.Values)
            {
                hash = hash * 31 + element.GetHashCode();
            }
        
            return hash;
        }

    }
    
}