using System.Text;
using LightSpeed.DbClient.Database;
using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Reflections;

namespace LightSpeed.DbClient.Postgresql.Database;

public class PostgresqlDeleteListQuery<T>(IFilters<T> filters, DatabaseObjectReflection reflection, IMapper mapper) : IQuery
    where T : IDatabaseElement
{
    private readonly QueryParameters _parameters = new ();

    public string GetQueryText()
    {
        _parameters.Clear();

        StringBuilder sb = new StringBuilder();
        
        List<IConnectedTable> connectedTables = reflection.ConnectedTables().ToList();
        foreach (var connectedTable in connectedTables)
        {
            sb.Append(ConnectedTableDeleteQuery(connectedTable));
            sb.Append(" ");
        }
        List<IConnectedTable> translationTables = reflection.TranslationTables().ToList();
        foreach (var connectedTable in translationTables)
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
        sb.Append($"FROM {reflection.MainTableReflection.QueryName()} as {reflection.MainTableReflection.QueryName()}");

        if (filters.HasMainTableFilters())
        {
            sb.Append($" ");
            sb.Append($"WHERE");
            sb.Append($" ");
            List<IFilter<T>> filters1 = filters.MainTableFilters().ToList();
            int index1 = 0;
            foreach (IFilter<T> filter in filters1)
            {
                var value = filter.Value();
                var type = filter.Type();
                value = mapper.MapToDatabaseValue(value, type);
                string parameterName = _parameters.Add(type, value);
                sb.Append($"{reflection.MainTableReflection.QueryName()}.{filter.Column().QueryName()} = {parameterName}");
                if (index1 < filters1.Count - 1)
                    sb.Append(" AND ");
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
        sb.Append($"{reflection.MainTableReflection.QueryName()}");
        sb.Append($" ");
        sb.Append($"WHERE");
        sb.Append($" ");
            
        List<IColumnReflection> ownerKeys = connectedTable.TableReflection().PartsOfOwnerKey().ToList();
        
        int index2 = 0;
        foreach (var keyPart in ownerKeys)
        {
            sb.Append($"{connectedTable.QueryName()}.{keyPart.QueryName()} = {reflection.MainTableReflection.QueryName()}.{keyPart.Relation()}");
            if (index2 < ownerKeys.Count - 1)
            {
                sb.Append(" ");
                sb.Append("AND");
                sb.Append(" ");
            }
            sb.Append(" ");
            index2++;
        }
            
        if (filters.Any())
        {
            List<IFilter<T>> filters1 = filters.ToList();
            int index4 = 0;
            foreach (Filter<T> filter in filters)
            {
                var value = filter.Value();
                var type = filter.Type();
                value = mapper.MapToDatabaseValue(value, type);
                string parameterName =_parameters.Add(type, value);
                sb.Append($"{reflection.MainTableReflection.QueryName()}.{filter.Column().QueryName()} {ComparisonOperatorConverter.Convert(filter.Operator())} {parameterName}");
                if (index4 < filters1.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
            }
        }
        sb.Append($";");
        return sb.ToString();
    }
    
}