using System.Text;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlSelectByKeyQuery: IQuery
{
    
    private readonly DatabaseObjectReflection _reflection;
    private readonly QueryParameters _parameters;
    private readonly IKey _key;
    
    public PostgresqlSelectByKeyQuery(DatabaseObjectReflection reflection, IKey key)
    {
        _reflection = reflection;
        _key = key;
        _parameters = new ();
    }

    public string GetQueryText()
    {
        
        _parameters.Clear();

        StringBuilder sb = new StringBuilder();
        sb.Append($"SELECT");
        sb.Append($" ");
        
        var columns = _reflection.MainTableReflection.Columns().ToList();
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            sb.Append($"{_reflection.MainTableReflection.QueryName()}.{column.QueryName()} as {column.QueryName()}");
            if (i < columns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        sb.Append($"FROM {_reflection.MainTableReflection.QueryName()} as {_reflection.MainTableReflection.QueryName()}");
        sb.Append($" ");
        sb.Append($"WHERE");
        sb.Append($" ");

        List<IKeyElement> keyElements = _key.KeyElements().ToList();
        
        for (int i = 0; i < keyElements.Count; i++)
        {
            
            var keyPart = keyElements[i];
            
            string parameterName = $"@{i}";
            var value = keyPart.Value();
            var type = value.GetType();
            
            _parameters.Add(new QueryParameter(parameterName, type, value));
            
            sb.Append($"{_reflection.MainTableReflection.QueryName()}.{keyPart.Column().QueryName()} = {parameterName}");
            if (i < keyElements.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
            
        }
        
        return sb.ToString();
        
    }

    public IQueryParameters Parameters()
    {
        return _parameters;
    }
    
}