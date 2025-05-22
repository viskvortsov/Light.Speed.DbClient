using System.Text;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlDeleteListQuery: IQuery
{
    
    private readonly DatabaseObjectReflection _reflection;
    private readonly QueryParameters _parameters;
    private readonly IEnumerable<IFilter> _filters;
    
    public PostgresqlDeleteListQuery(IEnumerable<IFilter> filters, DatabaseObjectReflection reflection)
    {
        _reflection = reflection;
        _filters = filters;
        _parameters = new ();
    }

    public string GetQueryText()
    {
        
        // TODO what about connected tables?
        
        _parameters.Clear();
        int i = 0;

        StringBuilder sb = new StringBuilder();
        sb.Append($"DELETE");
        sb.Append($" ");
        sb.Append($"FROM {_reflection.MainTableReflection.QueryName()} as {_reflection.MainTableReflection.QueryName()}");

        if (_filters.Any())
        {
            sb.Append($" ");
            sb.Append($"WHERE");
            sb.Append($" ");
            List<IFilter> filters = _filters.ToList();
            int index1 = 0;
            foreach (IFilter filter in filters)
            {
                string parameterName = $"@{i}";
                i++;
                var value = filter.Value();
                var type = value.GetType();
            
                _parameters.Add(new QueryParameter(parameterName, type, value));
            
                sb.Append($"{_reflection.MainTableReflection.QueryName()}.{filter.Column().QueryName()} = {parameterName}");
                if (index1 < filters.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
                index1++;
            }
        }
        else
        {
            sb.Append($" ");
            sb.Append($"WHERE true");
        }
        
        sb.Append($";");
        sb.Append($" ");
        
        List<ITableReflection> connectedTables = _reflection.ConnectedTables().ToList();

        foreach (var connectedTable in connectedTables)
        {
            sb.Append($"DELETE");
            sb.Append($" ");
            sb.Append($"FROM {connectedTable.QueryName()}");
            sb.Append($" ");
            sb.Append($"USING");
            sb.Append($" ");
            sb.Append($"{_reflection.MainTableReflection.QueryName()}");
            sb.Append($" ");
            sb.Append($"WHERE");
            sb.Append($" ");
            
            List<IColumnReflection> ownerKeys = connectedTable.PartsOfOwnerKey().ToList();
        
            int index2 = 0;
            foreach (var keyPart in ownerKeys)
            {
                sb.Append($"{connectedTable.QueryName()}.{keyPart.QueryName()} = {_reflection.MainTableReflection.QueryName()}.{keyPart.Relation()}");
                if (index2 < ownerKeys.Count - 1)
                {
                    sb.Append(" ");
                    sb.Append("AND");
                    sb.Append(" ");
                }
                sb.Append(" ");
                index2++;
            }
            
            if (_filters.Any())
            {
                List<IFilter> filters = _filters.ToList();
                int index4 = 0;
                foreach (IFilter filter in _filters)
                {
                    
                    string parameterName = $"@{i}";
                    var value = filter.Value();
                    var type = value.GetType();
            
                    _parameters.Add(new QueryParameter(parameterName, type, value));
            
                    sb.Append($"{_reflection.MainTableReflection.QueryName()}.{filter.Column().QueryName()} {ComparisonOperatorConverter.Convert(filter.Operator())} {parameterName}");
                    if (index4 < filters.Count - 1)
                        sb.Append(", ");
                    sb.Append(" ");
                }
            }
            sb.Append($";");
        }
        
        return sb.ToString();
        
    }

    public IQueryParameters Parameters()
    {
        return _parameters;
    }
    
}