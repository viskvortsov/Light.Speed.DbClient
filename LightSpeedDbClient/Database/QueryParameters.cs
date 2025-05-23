using System.Collections;

namespace LightSpeedDbClient.Database;

public class QueryParameters : IQueryParameters
{
    
    private readonly List<IQueryParameter> _parameters = new();
    
    public IEnumerator<IQueryParameter> GetEnumerator()
    {
        return _parameters.GetEnumerator();
    }

    public string Add(Type type, object value)
    {
        int index = _parameters.Count;
        string parameterName = $"@{index}";
        _parameters.Add(new QueryParameter(parameterName, type, value));
        return parameterName;
    }

    public void Clear()
    {
        _parameters.Clear();
    }

    public int Count()
    {
        return _parameters.Count;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
}