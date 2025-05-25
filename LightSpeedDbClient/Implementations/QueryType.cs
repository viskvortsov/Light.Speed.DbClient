using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public class QueryType(string type) : IQueryType
{
    public string Type()
    {
        return type;
    }
    
}