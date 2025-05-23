using System.Collections;
using System.Text;

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
        string finalName;
        int index = _parameters.Count;
        if (value is IList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            IList values = (IList) value;
            int i = 0;
            foreach (var innerValue in values)
            {
                string parameterName = $"@{index}";
                sb.Append(parameterName);
                _parameters.Add(new QueryParameter(parameterName, innerValue.GetType(), innerValue));
                index++;
                if (i < values.Count - 1)
                    sb.Append(",");
                i++;
            }
            sb.Append(")");
            finalName = sb.ToString();
        }
        else
        {
            string parameterName = $"@{index}";
            _parameters.Add(new QueryParameter(parameterName, type, value));
            finalName = parameterName;
        }
        return finalName;
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