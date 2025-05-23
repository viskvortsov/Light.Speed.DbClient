using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public class Key : IKey
{
    internal List<KeyElement> _keyElements;
    
    public Key()
    {
    }
    public Key(List<KeyElement> elements)
    {
        _keyElements = elements;
    }
    public IEnumerable<IKeyElement> KeyElements()
    {
        return _keyElements;
    }
}