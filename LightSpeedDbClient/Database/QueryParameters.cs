using System.Collections;

namespace LightSpeedDbClient.Database;

public class QueryParameters : IQueryParameters
{
    
    private readonly List<IQueryParameter> _parameters = new();
    
    public IEnumerator<IQueryParameter> GetEnumerator()
    {
        return _parameters.GetEnumerator();
    }

    public void Add(IQueryParameter parameter)
    {
        _parameters.Add(parameter);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
}