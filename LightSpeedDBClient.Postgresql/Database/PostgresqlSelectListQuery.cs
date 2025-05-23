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
        sb.Append(MainRowSelectQuery());
        return sb.ToString();
        
    }

    public IQueryParameters Parameters()
    {
        return _parameters;
    }

    private string MainRowSelectQuery()
    {
        
        StringBuilder sb = new StringBuilder();
        sb.Append($"SELECT");
        sb.Append($" ");
        
        var columns = _reflection.MainTableReflection.Columns().ToList();
        // main fields
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            sb.Append($"{_reflection.MainTableReflection.QueryName()}.{column.QueryName()} as {column.QueryName()}");
            if (i < columns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        
        // additional fields
        var columnsWithAdditionalInfo = _reflection.MainTableReflection.ColumnsWithAdditionalInfo();
        Dictionary<IColumnReflection, IColumnReflection> allAdditionalFields = new ();
        foreach (var column in columnsWithAdditionalInfo)
        {
            foreach (IColumnReflection additionalField in column.AdditionalFields())
            {
                allAdditionalFields.Add(additionalField, column);
            }
        }
        
        if (allAdditionalFields.Count > 0)
            sb.Append(",");

        int index0 = 0;
        foreach (var keyValue in allAdditionalFields)
        {
            IColumnReflection additionalField = keyValue.Key;
            IColumnReflection column = keyValue.Value;
            ITableReflection foreignKeyTable = column.ForeignKeyTable();
            sb.Append($"{foreignKeyTable.QueryName()}.{additionalField.QueryName()} as {foreignKeyTable.QueryName()}_{additionalField.QueryName()}");
            if (index0 < allAdditionalFields.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
            index0++;
        }
        
        // Main table
        sb.Append($"FROM {_reflection.MainTableReflection.QueryName()} as {_reflection.MainTableReflection.QueryName()}");
        
        // Additional tables
        foreach (var column in columnsWithAdditionalInfo)
        {
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{column.ForeignKeyTable().QueryName()}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{column.ForeignKeyTable().QueryName()}.{column.ForeignKeyColumn().QueryName()}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{_reflection.MainTableReflection.QueryName()}.{column.QueryName()}");
        }

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
        sb.Append(";");
        return sb.ToString();
        
    }
    
}