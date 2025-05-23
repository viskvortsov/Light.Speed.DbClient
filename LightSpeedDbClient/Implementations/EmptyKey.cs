using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public class EmptyKey : IKey
{
    public IEnumerable<IKeyElement> KeyElements()
    {
        return new List<IKeyElement>();
    }

    public object GetValue(string name)
    {
        throw null;
    }
    
}