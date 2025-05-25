using System.Text;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlSelectListQuery<T>: IQuery where T : IDatabaseElement
{
    
    private readonly DatabaseObjectReflection _reflection;
    private readonly int? _page;
    private readonly int? _limit;
    private readonly bool _usePagination;
    private readonly QueryParameters _parameters;
    private readonly IFilters<T> _filters;
    
    public PostgresqlSelectListQuery(IFilters<T> filters, DatabaseObjectReflection reflection, int? page = null, int? limit = null)
    {
        _reflection = reflection;
        _page = page;
        _limit = limit;

        if (_page <= 0)
            throw new PageValueException($"Page value should be more then 0.");
        
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

        if (_filters.HasConnectedTableFilters())
        {
            sb.Append(MainRowSelectFilterConnectedTableQuery());
            sb.Append(MainRowSelectPrefilteredWithConnectedTableQuery());
        }
        else
        {
            sb.Append(MainRowSelectQuery()); 
        }
        
        return sb.ToString();
        
    }

    public IQueryParameters Parameters()
    {
        return _parameters;
    }
    
    private string MainRowSelectFilterConnectedTableQuery()
    {
        
        StringBuilder sb = new StringBuilder();
        
        sb.Append($"WITH temp_table AS");
        sb.Append($"(");
        sb.Append($"SELECT");
        sb.Append($" ");
        
        var columns = _reflection.MainTableReflection.PartsOfPrimaryKey().ToList();
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
        
        // Main table
        sb.Append($"FROM {_reflection.MainTableReflection.QueryName()} as {_reflection.MainTableReflection.QueryName()}");
        
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
            sb.Append($"{_reflection.MainTableReflection.QueryName()}.{column.QueryName()}");
        }

        IFilters<T> filters = _filters.ConnectedTableFilters();
        HashSet<ITableReflection> uniqueTables = new HashSet<ITableReflection>();
        foreach (var filter in filters)
        {
            uniqueTables.Add(filter.Column().Table());
        }

        foreach (var table in uniqueTables)
        {
            
            sb.Append(" ");
            sb.Append("INNER JOIN");
            sb.Append(" ");
            sb.Append($"{table.QueryName()}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            
            List<IColumnReflection> ownerKeys = table.PartsOfOwnerKey().ToList();
            for (int i = 0; i < ownerKeys.Count; i++)
            {
                var keyPart = ownerKeys[i];
                sb.Append($"{_reflection.MainTableReflection.QueryName()}.{keyPart.Relation()}");
                sb.Append($" ");
                sb.Append($"=");
                sb.Append($" ");
                sb.Append($"{table.QueryName()}.{keyPart.QueryName()}");
                if (i < ownerKeys.Count - 1)
                {
                    sb.Append(" ");
                    sb.Append("AND");
                    sb.Append(" ");
                }
                sb.Append(" ");
                sb.Append("AND");
                sb.Append(" ");
            }
            
            Filters<T> thisTableFilters = new Filters<T>();
            foreach (var filter in _filters.ConnectedTableFilters())
            {
                if (filter.Column().Table() == table)
                    thisTableFilters.Add(filter);
            }
            
            int index10 = 0;
            foreach (var filter in thisTableFilters)
            {
                var value = filter.Value();
                var type = filter.Type();
                string parameterName = _parameters.Add(type, value);
                
                sb.Append($"{filter.Column().Table().QueryName()}.{filter.Column().QueryName()}");
                sb.Append(" ");
                sb.Append($"{ComparisonOperatorConverter.Convert(filter.Operator())}");
                sb.Append(" ");
                sb.Append($"{parameterName}");
                if (index10 < thisTableFilters.Count() - 1)
                {
                    sb.Append(" ");
                    sb.Append("AND");
                    sb.Append(" ");
                }
                sb.Append(" ");
                index10++;
            }
            
        }
        
        if (_filters.HasMainTableFilters())
        {
            sb.Append($" ");
            sb.Append($"WHERE");
            sb.Append($" ");
            List<Filter<T>> mainTableFilters = _filters.MainTableFilters().ToList();
            for (int i = 0; i < mainTableFilters.Count; i++)
            {
                var filter = mainTableFilters[i];
                var value = filter.Value();
                var type = filter.Type();
                string parameterName = _parameters.Add(type, value);
                sb.Append($"{_reflection.MainTableReflection.QueryName()}.{filter.Column().QueryName()} {ComparisonOperatorConverter.Convert(filter.Operator())} {parameterName}");
                if (i < mainTableFilters.Count - 1)
                {
                    sb.Append(" ");
                    sb.Append("AND");
                    sb.Append(" ");
                }
                sb.Append(" ");
            }
        }
        
        sb.Append("GROUP BY");
        sb.Append(" ");
        // main fields
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            sb.Append($"{_reflection.MainTableReflection.QueryName()}.{column.QueryName()}");
            if (i < columns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        
        if (_usePagination)
        {
            
            int page = (int)(_page - 1)!;
            
            sb.Append(" ");
            sb.Append($"OFFSET {page * _limit} ROWS FETCH NEXT {_limit} ROWS ONLY");
            sb.Append(" ");

        }
        sb.Append($")");
        sb.Append(" ");
        return sb.ToString();
        
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
        
        // Main table
        sb.Append($"FROM {_reflection.MainTableReflection.QueryName()} as {_reflection.MainTableReflection.QueryName()}");
        
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
            sb.Append($"{_reflection.MainTableReflection.QueryName()}.{column.QueryName()}");
        }

        if (_filters.Any())
        {
            sb.Append($" ");
            sb.Append($"WHERE");
            sb.Append($" ");
            List<Filter<T>> filters = _filters.ToList();
            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                var value = filter.Value();
                var type = filter.Type();
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
    
    private string MainRowSelectPrefilteredWithConnectedTableQuery()
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
        
        // Main table
        sb.Append($"FROM {_reflection.MainTableReflection.QueryName()} as {_reflection.MainTableReflection.QueryName()}");
        sb.Append($" ");
        sb.Append($"WHERE");
        sb.Append($" ");
        
        sb.Append($"(");
        var keyFields = _reflection.MainTableReflection.PartsOfPrimaryKey().ToList();
        // main fields
        for (int i = 0; i < keyFields.Count; i++)
        {
            var column = columns[i];
            sb.Append($"{_reflection.MainTableReflection.QueryName()}.{column.QueryName()}");
            if (i < keyFields.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        sb.Append($")");
        sb.Append($" ");
        sb.Append($"IN");
        sb.Append($" ");
        
        sb.Append($"(");
        sb.Append($"SELECT");
        sb.Append($" ");
        // main fields
        for (int i = 0; i < keyFields.Count; i++)
        {
            var column = columns[i];
            sb.Append($"temp_table.{column.QueryName()}");
            if (i < keyFields.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        sb.Append($"FROM temp_table");
        sb.Append($")");
        
        sb.Append(";");
        return sb.ToString();
        
    }
    
}