using System.Text;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlSelectListObjectsQuery: IQuery
{
    
    private readonly DatabaseObjectReflection _reflection;
    private readonly int? _page;
    private readonly int? _limit;
    private readonly bool _usePagination;
    private readonly QueryParameters _parameters;
    private readonly IEnumerable<IFilter> _filters;
    
    public PostgresqlSelectListObjectsQuery(IEnumerable<IFilter> filters, DatabaseObjectReflection reflection, int? page = null, int? limit = null)
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
        
        string uniqueId = Guid.NewGuid().ToString("N");
        string shortId = uniqueId.Substring(0, 16);
        string tableName = $"temp_{shortId}";

        StringBuilder sb = new StringBuilder();
        sb.Append(MainRowTemporaryTableQuery(tableName));
        sb.Append(" ");
        sb.Append(MainRowToTemporaryTableQuery(tableName));
        sb.Append(" ");
        sb.Append(MainRowFromTemporaryTableQuery(tableName));
        sb.Append(" ");
        List<IConnectedTable> connectedTables = _reflection.ConnectedTables().ToList();
        foreach (var connectedTable in connectedTables)
        {
            sb.Append(ConnectedTableSelectQuery(connectedTable, tableName));
            sb.Append(" ");
        }
        sb.Append(DropTemporaryTableQuery(tableName));
        return sb.ToString();
        
    }

    public IQueryParameters Parameters()
    {
        return _parameters;
    }

    private string MainRowTemporaryTableQuery(string tableName)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"CREATE TEMPORARY TABLE");
        sb.Append($" ");
        sb.Append($"{tableName}");
        sb.Append($"(");
        var columns = _reflection.MainTableReflection.Columns().ToList();
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            sb.Append($"{column.QueryName()} {column.QueryType().Type()}");
            if (i < columns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        sb.Append($")");
        sb.Append($";");
        return sb.ToString();
    }
    
    private string DropTemporaryTableQuery(string tableName)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"DROP TABLE");
        sb.Append($" ");
        sb.Append($"{tableName}");
        sb.Append($";");
        return sb.ToString();
    }

    private string MainRowToTemporaryTableQuery(string tableName)
    {
        
        StringBuilder sb = new StringBuilder();
        sb.Append($"INSERT INTO");
        sb.Append($" ");
        sb.Append($"{tableName}");
        sb.Append($"(");
        var columns = _reflection.MainTableReflection.Columns().ToList();
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            sb.Append($"{column.QueryName()}");
            if (i < columns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        sb.Append($")");
        sb.Append($" ");

        sb.Append($"SELECT");
        sb.Append($" ");

        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            sb.Append($"{column.QueryName()}");
            if (i < columns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        sb.Append($"FROM {_reflection.MainTableReflection.QueryName()}");

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
    
    private string MainRowFromTemporaryTableQuery(string tableName)
    {
        
        StringBuilder sb = new StringBuilder();
        
        sb.Append($"SELECT");
        sb.Append($" ");

        var columns = _reflection.MainTableReflection.Columns().ToList();
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            sb.Append($"{tableName}.{column.QueryName()}");
            if (i < columns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        
        // additional fields
        var additionalFields = _reflection.MainTableReflection.AdditionalFields().ToList();
        
        if (additionalFields.Count > 0)
            sb.Append(",");

        int index0 = 0;
        foreach (var additionalField in additionalFields)
        {
            ITableReflection foreignKeyTable = additionalField.ForeignKeyTable();
            sb.Append($"{foreignKeyTable.QueryName()}.{additionalField.QueryName()} as {foreignKeyTable.QueryName()}_{additionalField.QueryName()}");
            if (index0 < additionalFields.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
            index0++;
        }
        
        sb.Append($"FROM {tableName}");
        
        // Additional tables
        var additionalTables = _reflection.MainTableReflection.ColumnsWithForeignKey().ToList();
        foreach (var column in additionalTables)
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
            sb.Append($"{tableName}.{column.QueryName()}");
        }
        
        sb.Append($";");
        
        return sb.ToString();
        
    }
    
    private string ConnectedTableSelectQuery(IConnectedTable connectedTable, string tableName)
    {
        
        StringBuilder sb = new StringBuilder();
        sb.Append($"SELECT");
        sb.Append($" ");
            
        var connectedTableColumns = connectedTable.TableReflection().Columns().ToList();
        for (int i = 0; i < connectedTableColumns.Count; i++)
        {
            var column = connectedTableColumns[i];
            sb.Append($"{connectedTable.QueryName()}.{column.QueryName()} as {column.QueryName()}");
            if (i < connectedTableColumns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        sb.Append($"FROM {connectedTable.QueryName()} as {connectedTable.QueryName()}");
        sb.Append($" ");
        sb.Append($"WHERE");
        sb.Append($" ");
            
        sb.Append("(");
        List<IColumnReflection> ownerKeys = connectedTable.TableReflection().PartsOfOwnerKey().ToList();
        for (int i = 0; i < ownerKeys.Count; i++)
        {
            var keyPart = ownerKeys[i];
            sb.Append($"{connectedTable.QueryName()}.{keyPart.QueryName()}");
            if (i < ownerKeys.Count - 1)
            {
                sb.Append(",");
            }
        }
        sb.Append(")");
        sb.Append(" ");
        sb.Append("IN");
        sb.Append(" ");
        sb.Append("(");
        
        sb.Append($"SELECT");
        sb.Append($" ");
        for (int i = 0; i < ownerKeys.Count; i++)
        {
            var keyPart = ownerKeys[i];
            sb.Append($"{tableName}.{keyPart.Relation()}");
            if (i < ownerKeys.Count - 1)
            {
                sb.Append(",");
            }
        }
        sb.Append(" ");
        sb.Append($"FROM {tableName}");
        
        sb.Append(")");
        sb.Append(";");
        
        return sb.ToString();
        
    }
    
}