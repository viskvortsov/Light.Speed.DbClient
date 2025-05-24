using System.Text;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlDeleteListQuery<E>: IQuery where E : IDatabaseElement
{
    
    private readonly DatabaseObjectReflection _reflection;
    private readonly QueryParameters _parameters;
    private readonly IFilters<E> _filters;
    
    public PostgresqlDeleteListQuery(IFilters<E> filters, DatabaseObjectReflection reflection)
    {
        _reflection = reflection;
        _filters = filters;
        _parameters = new ();
    }

    public string GetQueryText()
    {
        
        _parameters.Clear();
        int i = 0;

        StringBuilder sb = new StringBuilder();
        
        List<IConnectedTable> connectedTables = _reflection.ConnectedTables().ToList();
        foreach (var connectedTable in connectedTables)
        {
            sb.Append(ConnectedTableDeleteQuery(connectedTable));
            sb.Append(" ");
        }
        
        sb.Append(MainRowDeleteQuery());
        sb.Append($" ");
        
        return sb.ToString();
        
    }

    public IQueryParameters Parameters()
    {
        return _parameters;
    }

    private string MainRowDeleteQuery()
    {
        
        // TODO Add connected tables filter
        
        StringBuilder sb = new StringBuilder();
        sb.Append($"DELETE");
        sb.Append($" ");
        sb.Append($"FROM {_reflection.MainTableReflection.QueryName()} as {_reflection.MainTableReflection.QueryName()}");

        if (_filters.HasMainTableFilters())
        {
            sb.Append($" ");
            sb.Append($"WHERE");
            sb.Append($" ");
            List<Filter<E>> filters = _filters.MainTableFilters().ToList();
            int index1 = 0;
            foreach (Filter<E> filter in filters)
            {
                var value = filter.Value();
                var type = value.GetType();
                string parameterName = _parameters.Add(type, value);
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
        return sb.ToString();
    }

    private string ConnectedTableDeleteQuery(IConnectedTable connectedTable)
    {
        StringBuilder sb = new StringBuilder();
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
            
        List<IColumnReflection> ownerKeys = connectedTable.TableReflection().PartsOfOwnerKey().ToList();
        
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
            List<Filter<E>> filters = _filters.ToList();
            int index4 = 0;
            foreach (Filter<E> filter in _filters)
            {
                var value = filter.Value();
                var type = value.GetType();
                string parameterName =_parameters.Add(type, value);
                sb.Append($"{_reflection.MainTableReflection.QueryName()}.{filter.Column().QueryName()} {ComparisonOperatorConverter.Convert(filter.Operator())} {parameterName}");
                if (index4 < filters.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
            }
        }
        sb.Append($";");
        return sb.ToString();
    }
    
}