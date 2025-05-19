using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public abstract class DatabaseObject : IDatabaseObject
{
    
    private IKey key;
    private IRow row;
    private IObjectArrays arrays;

}