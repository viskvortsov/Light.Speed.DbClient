using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.Implementations;

public class EmptyKey : IKey
{
    public IEnumerable<IKeyElement> KeyElements()
    {
        return new List<IKeyElement>();
    }

    public object? GetValue(string name)
    {
        return null;
    }
    
}