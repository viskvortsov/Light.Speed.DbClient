using System.Text;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlSelectListObjectsQuery<T>: IQuery where T : IDatabaseElement
{
    
    private readonly DatabaseObjectReflection _reflection;
    private readonly int? _page;
    private readonly int? _limit;
    private readonly bool _usePagination;
    private readonly QueryParameters _parameters;
    private readonly IFilters<T> _filters;
    private readonly ISorting<T> _sorting;
    private readonly IMapper _mapper;
    private readonly ModelType _mode;
    
    public PostgresqlSelectListObjectsQuery(
        IFilters<T> filters,
        ISorting<T> sorting,
        ModelType modelType, 
        DatabaseObjectReflection reflection, 
        IMapper mapper, 
        int? page = null, 
        int? limit = null)
    {

        if (modelType != ModelType.Reference && modelType != ModelType.Object)
        {
            throw new NotSupportedException(); // TODO
        }
        
        _reflection = reflection;
        _page = page;
        _limit = limit;
        _mapper = mapper;
        _mode = modelType;

        if (_page <= 0)
            throw new PageValueException("Page value should be more then 0.");
        
        if (page != null && limit == null)
            limit = 10; // TODO
        
        _usePagination = page != null && limit != null;
        _filters = filters;
        _sorting = sorting;
        _parameters = new ();
        
    }

    public string GetQueryText()
    {
        
        _parameters.Clear();
        
        string uniqueId = Guid.NewGuid().ToString("N");
        string shortId = uniqueId.Substring(0, 16);
        string tableName = $"temp_{shortId}";

        StringBuilder sb = new StringBuilder();
        
        if (_filters.HasConnectedTableFilters())
        {
            sb.Append(MainRowTemporaryTableQuery(tableName));
            sb.Append(" ");
            sb.Append(MainRowSelectFilterConnectedTableQuery());
            sb.Append(" ");
            sb.Append(MainRowToTemporaryTablePrefilteredWithConnectedTableQuery(tableName));
            sb.Append(" ");
        }
        else
        {
            sb.Append(MainRowTemporaryTableQuery(tableName));
            sb.Append(" ");
            sb.Append(MainRowToTemporaryTableQuery(tableName));
            sb.Append(" ");
        }
        
        sb.Append(MainRowFromTemporaryTableQuery(tableName));
        sb.Append(" ");
        if (_mode == ModelType.Object)
        {
            List<IConnectedTable> connectedTables = _reflection.ConnectedTables().ToList();
            foreach (var connectedTable in connectedTables)
            {
                sb.Append(ConnectedTableSelectQuery(connectedTable, tableName));
                sb.Append(" ");
            }
        }
        
        List<IConnectedTable> translationTables = _reflection.TranslationTables().ToList();
        foreach (var connectedTable in translationTables)
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
            ITableReflection? foreignKeyTable = additionalField.ForeignKeyTable();
            if (foreignKeyTable == null)
                throw new ReflectionException("Additional field has no foreign key table.");
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
            ITableReflection? tableReflection = column.ForeignKeyTable();
            if (tableReflection == null)
                throw new ReflectionException($"Foreign key table not found for {column.ForeignKeyName()}");
            
            IColumnReflection? columnReflection = column.ForeignKeyColumn();
            if (columnReflection == null)
                throw new ReflectionException($"Foreign key column not found for {column.ForeignKeyName()}");

            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{tableReflection.QueryName()}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{tableReflection.QueryName()}.{columnReflection.QueryName()}");
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
                value = _mapper.MapToDatabaseValue(value, type);
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
            List<IFilter<T>> mainTableFilters = _filters.MainTableFilters().ToList();
            for (int i = 0; i < mainTableFilters.Count; i++)
            {
                var filter = mainTableFilters[i];
                var type = filter.Type();
                var value = filter.Value();
                value = _mapper.MapToDatabaseValue(value, type);
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
            string typeName = PostgresqlDefaultSettings.GetSqlDbTypeName(column.Type());
            sb.Append($"{column.QueryName()} {typeName}");
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
            List<IFilter<T>> filters = _filters.ToList();
            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                var value = filter.Value();
                var type = filter.Type();
                value = _mapper.MapToDatabaseValue(value, type);
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
    
    private string MainRowToTemporaryTablePrefilteredWithConnectedTableQuery(string tableName)
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
        
        // additional fields
        var additionalFields = connectedTable.TableReflection().AdditionalFields().ToList();
        
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

        List<String[]> allTranslationJoins = new List<String[]>();
        var translatableFields = connectedTable.TableReflection().TranslatableColumns().ToList();
        if (translatableFields.Count > 0)
            sb.Append(", ");
        int index = 0;
        foreach (var translatableField in translatableFields)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                index++;
                continue;
            }
            var field = translatableField.QueryName();
            var table = translatableField.ForeignKeyTable().QueryName();
            var translationsTable = translatableField.ForeignKeyTable().TranslationsTableQueryName();
            allTranslationJoins.Add([translationsTable, table, field]);
            sb.Append("jsonb_agg");
            sb.Append("(");
            sb.Append("jsonb_build_object");
            sb.Append("(");
            sb.Append("'language_id'");
            sb.Append(",");
            sb.Append($"{translationsTable}.language_id");
            sb.Append(",");
            sb.Append("'content_id'");
            sb.Append(",");
            sb.Append($"{translationsTable}.content_id");
            sb.Append(",");
            sb.Append("'content'");
            sb.Append(",");
            sb.Append($"{translationsTable}.content");
            sb.Append(")");
            sb.Append(")");
            sb.Append(" ");
            sb.Append("as");
            sb.Append(" ");
            sb.Append("a"); // TODO
            if (index < translatableFields.Count - 1)
                sb.Append(",");
            index++;
        }
        sb.Append(" ");
        
        sb.Append($"FROM {connectedTable.QueryName()} as {connectedTable.QueryName()}");
        sb.Append($" ");
        // Additional tables
        var additionalTables = connectedTable.TableReflection().ColumnsWithForeignKey().ToList();
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
            sb.Append($"{connectedTable.QueryName()}.{column.QueryName()}");
        }
        sb.Append($" ");
        foreach (var join in allTranslationJoins)
        {
            var translationsTable = join[0];
            var table = join[1];
            var field = join[2];
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{translationsTable}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{table}.{field}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{translationsTable}.content_id");
        }
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
        if (translatableFields.Count > 0)
        {
            sb.Append(" ");
            sb.Append("GROUP BY");
            sb.Append(" ");
            for (int i = 0; i < connectedTableColumns.Count; i++)
            {
                var column = connectedTableColumns[i];
                sb.Append($"{connectedTable.TableReflection().QueryName()}.{column.QueryName()}");
                if (i < connectedTableColumns.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
            }

            if (additionalFields.Count > 0)
            {
                sb.Append(",");
                sb.Append(" ");
            }
            
            for (int i = 0; i < additionalFields.Count; i++)
            {
                var additionalField = additionalFields[i];
                ITableReflection foreignKeyTable = additionalField.ForeignKeyTable();
                sb.Append($"{foreignKeyTable.QueryName()}.{additionalField.QueryName()}");
                if (index0 < additionalFields.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
            }
        }
        sb.Append(";");
        
        return sb.ToString();
        
    }
    
}