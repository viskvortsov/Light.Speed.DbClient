using System.Text;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlSelectListQuery: IQuery
{
    
    private readonly DatabaseObjectReflection _reflection;
    private readonly int? _page;
    private readonly int? _limit;
    private readonly bool _usePagination;
    private readonly QueryParameters _parameters;
    private readonly IEnumerable<IFilter> _filters;
    
    public PostgresqlSelectListQuery(IEnumerable<IFilter> filters, DatabaseObjectReflection reflection, int? page = null, int? limit = null)
    {
        _reflection = reflection;
        _page = page;
        _limit = limit;

        if (_page == 0)
            throw new PageValueException();
        
        if (page != null && limit == null)
            limit = 10; // TODO
        
        _usePagination = page != null && limit != null;
        _filters = filters;
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
            sb.Append($"{column.QueryName()} as {column.QueryName()}");
            if (i < columns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        sb.Append($"FROM {_reflection.MainTableReflection.QueryName()} as {_reflection.MainTableReflection.QueryName()}");

        if (_filters.Any())
        {
            sb.Append($" ");
            sb.Append($"WHERE");
            sb.Append($" ");
            List<IFilter> filters = _filters.ToList();
            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                var value = filter.Value();
                var type = value.GetType();
                string parameterName = _parameters.Add(type, value);
                sb.Append($"{_reflection.MainTableReflection.QueryName()}.{filter.Column().QueryName()} {ComparisonOperatorConverter.Convert(filter.Operator())} {parameterName}");
                if (i < filters.Count - 1)
                {
                    sb.Append(" ");
                    sb.Append("AND");
                    sb.Append(" ");
                }
                sb.Append(" ");
            }
        }
        
        if (_usePagination)
        {
            
            int page = (int)(_page - 1)!;
            
            sb.Append(" ");
            sb.Append($"OFFSET {page * _limit} ROWS FETCH NEXT {_limit} ROWS ONLY");
            sb.Append(" ");

        }
            
        return sb.ToString();
        
    }

    public IQueryParameters Parameters()
    {
        return _parameters;
    }
    
}