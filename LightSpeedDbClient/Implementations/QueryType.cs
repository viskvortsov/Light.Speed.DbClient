using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public class QueryType : IQueryType
{
    private readonly string _queryType;

    public QueryType(string type)
    {
        _queryType = type;
    }

    public string Type()
    {
        return _queryType;
    }
    
}