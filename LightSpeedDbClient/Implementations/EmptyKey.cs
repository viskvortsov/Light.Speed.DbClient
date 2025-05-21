using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Implementations;

public class EmptyKey : IKey
{
    public IEnumerable<IKeyElement> KeyElements()
    {
        return new List<IKeyElement>();
    }
    
}