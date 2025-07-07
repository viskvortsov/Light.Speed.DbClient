using System.Collections;
using System.Text;
using LightSpeed.DbClient.Database;
using LightSpeed.DbClient.Exceptions;
using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Reflections;

namespace LightSpeed.DbClient.Postgresql.Database;

public class PostgresqlSelectListObjectsQuery<T>: IQuery where T : IDatabaseElement
{
    
    private readonly Dictionary<string, string> _tableReplacements = new ();
    private readonly Dictionary<string, string> _translationsTableReplacements = new ();
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
            throw new NotSupportedException("Only models Reference and Object are supported");
        }
        
        _reflection = reflection;
        _page = page;
        _limit = limit;
        _mapper = mapper;
        _mode = modelType;

        if (_page <= 0)
            throw new PageValueException("Page value should be more then 0.");
        
        if (page != null && limit == null)
            limit = 10;
        
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
        
        if (_filters.HasConnectedTableFilters() || _filters.HasTranslationFieldsFilters())
        {
            FillReplacements(_reflection.MainTableReflection);
            sb.Append(MainRowTemporaryTableQuery(tableName));
            sb.Append(" ");
            FillReplacements(_reflection.MainTableReflection);
            sb.Append(MainRowSelectFilterConnectedTableQuery());
            sb.Append(" ");
            FillReplacements(_reflection.MainTableReflection);
            sb.Append(MainRowToTemporaryTablePrefilteredWithConnectedTableQuery(tableName));
            sb.Append(" ");
        }
        else
        {
            FillReplacements(_reflection.MainTableReflection);
            sb.Append(MainRowTemporaryTableQuery(tableName));
            sb.Append(" ");
            FillReplacements(_reflection.MainTableReflection);
            sb.Append(MainRowToTemporaryTableQuery(tableName));
            sb.Append(" ");
        }
        
        FillReplacements(_reflection.MainTableReflection);
        sb.Append(MainRowFromTemporaryTableQuery(tableName));
        sb.Append(" ");
        if (_mode == ModelType.Object)
        {
            List<IConnectedTable> connectedTables = _reflection.ConnectedTables().ToList();
            foreach (var connectedTable in connectedTables)
            {
                FillReplacements(connectedTable.TableReflection());
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
        
        // Main table
        sb.Append($"FROM {_reflection.MainTableReflection.QueryName()} as {_reflection.MainTableReflection.QueryName()}");
        
        IFilters<T> filters = _filters.ConnectedTableFilters();
        Dictionary<ITableReflection, string> uniqueTables = new Dictionary<ITableReflection, string>();
        int l = 0;
        foreach (var filter in filters)
        {
            if (!uniqueTables.ContainsKey(filter.Column().Table()))
            {
                uniqueTables.Add(filter.Column().Table(), $"{filter.Column().Table().QueryName()}_{l}");
                l++;
            }
        }

        foreach (var table in uniqueTables)
        {
            
            sb.Append(" ");
            sb.Append("INNER JOIN");
            sb.Append(" ");
            sb.Append($"{table.Key.QueryName()} as {table.Value}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            
            List<IColumnReflection> ownerKeys = table.Key.PartsOfOwnerKey().ToList();
            for (int i = 0; i < ownerKeys.Count; i++)
            {
                var keyPart = ownerKeys[i];
                sb.Append($"{_reflection.MainTableReflection.QueryName()}.{keyPart.Relation()}");
                sb.Append($" ");
                sb.Append($"=");
                sb.Append($" ");
                sb.Append($"{table.Value}.{keyPart.QueryName()}");
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
                if (filter.Column().Table() == table.Key)
                    thisTableFilters.Add(filter);
            }
            
            int index10 = 0;
            foreach (var filter in thisTableFilters)
            {
                var value = filter.Value();
                var type = filter.Type();
                value = _mapper.MapToDatabaseValue(value, type);
                string parameterName = _parameters.Add(type, value);
                
                sb.Append($"{table.Value}.{filter.Column().QueryName()}");
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
        
        List<IColumnReflection> allTranslationJoins = new List<IColumnReflection>();
        if (_filters.HasTranslationFieldsFilters())
        {
            foreach (var filter in _filters)
            {
                if (filter.IsTranslationFieldsFilter())
                {
                    if (filter.Value()?.GetType() != typeof(String))
                    {
                        throw new NotSupportedException("Only string values are supported as filter values for translatable fields");
                    }
                    var field = filter.Column().QueryName();
                    var table = filter.Column().Table().QueryName();
                    var translationsTable = filter.Column().Table().TranslationsTableQueryName();
                    if (translationsTable == null)
                    {
                        throw new ReflectionException($"Translations table not found for column {field} in table {table}");
                    }
                    var value = (String) filter.Value()!;
                    var comparisonOperator = ComparisonOperatorConverter.Convert(filter.Operator());
                    sb.Append(" ");
                    sb.Append("INNER JOIN");
                    sb.Append(" ");
                    sb.Append($"{translationsTable}"); // this seems to be main translation table so maybe synonym is redundant
                    sb.Append(" ");
                    sb.Append("ON");
                    sb.Append(" ");
                    sb.Append($"{table}.{field}");
                    sb.Append(" ");
                    sb.Append("=");
                    sb.Append(" ");
                    sb.Append($"{translationsTable}.content_id");
                    sb.Append(" ");
                    sb.Append("AND");
                    sb.Append(" ");
                    sb.Append($"lower({translationsTable}.content)");
                    sb.Append(" ");
                    sb.Append(comparisonOperator);
                    sb.Append(" ");
                    string parameterName = _parameters.Add(typeof(string), value.ToLower());
                    sb.Append(parameterName);
                }
            }
        }
        
        sb.Append($" ");

        if (_filters.HasMainTableNotTranslatableFilters())
        {
            sb.Append($" ");
            sb.Append($"WHERE");
            sb.Append($" ");
            List<IFilter<T>> mainTableFilters = _filters.MainTableNotTranslatableFilters().ToList();
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
        
        sb.Append(" ");
        sb.Append("GROUP BY");
        sb.Append(" ");
        int index100 = 0;
        foreach (var column in columns)
        {
            sb.Append($"{_reflection.MainTableReflection.QueryName()}.{column.QueryName()}");
            if (index100 < columns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
            index100++;
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
        
        if (_filters.HasMainTableFilters())
        {
            sb.Append($" ");
            sb.Append($"WHERE");
            sb.Append($" ");
            List<IFilter<T>> filters = _filters.ToList();
            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                if (!filter.IsTranslationFieldsFilter())
                {
                    var value = filter.Value();
                    var type = filter.Type();
                    var table = filter.Column().Table();
                    value = _mapper.MapToDatabaseValue(value, type);
                    object newValue = value;
                    
                    // All strings should be lower case
                    // Even if it is Array of strings 
                    // TODO extract to a helper
                    if (type == typeof(String))
                    {
                        if (value.GetType() == typeof(ICollection) ||
                            value.GetType().GetInterface(typeof(ICollection).FullName!) != null)
                        {
                            ICollection list = (value as ICollection)!;
                            ArrayList newList = new ArrayList();
                            foreach (string innerValue in list)
                            {
                                string lower = innerValue.ToLower();
                                newList.Add(lower);
                            }
                            newValue = newList;
                        }
                        else if (value is string)
                        {
                            newValue = ((String)value!).ToLower();
                        }
                        string parameterName = _parameters.Add(type, newValue);
                        sb.Append($"lower({table.QueryName()}.{filter.Column().QueryName()}) {ComparisonOperatorConverter.Convert(filter.Operator())} {parameterName}");

                    }
                    else
                    {
                        string parameterName = _parameters.Add(type, newValue);
                        sb.Append($"{table.QueryName()}.{filter.Column().QueryName()} {ComparisonOperatorConverter.Convert(filter.Operator())} {parameterName}");
                    }
                    
                    if (i < filters.Count - 1)
                    {
                        sb.Append(" ");
                        sb.Append("AND");
                        sb.Append(" ");
                    }
                    sb.Append(" ");
                }
            }
        }

        if (_sorting.HasMainTableSortingElements())
        {
            sb.Append(" ");
            sb.Append("ORDER BY");
            sb.Append(" ");
            List<ISortingElement<T>> sortingElements = _sorting.ToList();
            for (int i = 0; i < sortingElements.Count; i++)
            {
                var sortingElement = sortingElements[i];
                var column = sortingElement.Column();
                var table = column.Table();
                sb.Append($"{table.QueryName()}.{column.QueryName()} {sortingElement.DirectionAsString()}");
                if (i < sortingElements.Count - 1)
                {
                    sb.Append(",");
                }
                sb.Append(" ");
            }
        }
        
        sb.Append(" ");
        
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
    
    private string MainRowFromTemporaryTableQuery(string temporaryTableName)
    {
        
        var additionalTables = _reflection.MainTableReflection.ColumnsWithForeignKey().ToList();
        var additionalTables2 = _reflection.MainTableReflection.AdditionalFieldsWithForeignKey().ToList();
        var additionalColumns = _reflection.MainTableReflection.AdditionalFields().ToList();
        var additionalColumns2 = _reflection.MainTableReflection.AdditionalFields2().ToList();

        List<IColumnReflection> allTranslationJoins = new List<IColumnReflection>();
        var translatableFields = _reflection.MainTableReflection.TranslatableColumns().ToList();
        foreach (var translatableField in translatableFields)
        {
            if (translatableField.HasForeignKeyTable())
            {
                allTranslationJoins.Add(translatableField);
            }
        }
        
        List<IColumnReflection> allTranslationJoins2 = new List<IColumnReflection>();
        var translatableFields2 = _reflection.MainTableReflection.AdditionalTranslatableColumns().ToList();
        foreach (var translatableField in translatableFields2)
        {
            if (translatableField.HasForeignKeyTable())
            {
                allTranslationJoins2.Add(translatableField);
            }
        }
        
        StringBuilder sb = new StringBuilder();
        
        sb.Append($"SELECT");
        sb.Append($" ");

        var columns = _reflection.MainTableReflection.Columns().ToList();
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            sb.Append($"{temporaryTableName}.{column.QueryName()}");
            if (i < columns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        
        if (additionalColumns.Count > 0)
            sb.Append(",");

        int index0 = 0;
        foreach (var column in additionalColumns)
        {
            ITableReflection foreignKeyTable = column.ForeignKeyTable();
            sb.Append($"{GetTableSynonym(column)}.{column.QueryName()} as {foreignKeyTable.QueryName()}_{column.QueryName()}");
            if (index0 < additionalColumns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
            index0++;
        }
        
        // additional fields 2
        if (additionalColumns2.Count > 0)
            sb.Append(",");
        
        int index1 = 0;
        foreach (var column in additionalColumns2)
        {
            ITableReflection foreignKeyTable = column.ForeignKeyTable();
            sb.Append($"{GetAdditionalTableSynonym(column)}.{column.QueryName()} as {foreignKeyTable.QueryName()}_{column.QueryName()}");
            if (index1 < additionalColumns2.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
            index1++;
        }
        
        foreach (var translatableField in translatableFields)
        {
            if (translatableField.HasForeignKeyTable())
            {
                sb.Append(", ");
                break;
            }
        }
        
        int index = 0;
        foreach (var column in translatableFields)
        {
            if (!column.HasForeignKeyTable())
            {
                index++;
                continue;
            }
            var field = column.QueryName();
            var table = column.ForeignKeyTable().QueryName();
            var translationsTable = column.ForeignKeyTable().TranslationsTableQueryName();
            if (translationsTable == null)
            {
                throw new ReflectionException($"Translations table not found for column {field} in table {table}");
            }
            sb.Append("jsonb_agg");
            sb.Append("(");
            sb.Append("jsonb_build_object");
            sb.Append("(");
            sb.Append("'language_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(column)}.language_id");
            sb.Append(",");
            sb.Append("'content_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(column)}.content_id");
            sb.Append(",");
            sb.Append("'content'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(column)}.content");
            sb.Append(")");
            sb.Append(")");
            sb.Append(" ");
            sb.Append("as");
            sb.Append(" ");
            sb.Append($"{column.TranslationsQueryName()}");
            if (index < translatableFields.Count - 1)
                sb.Append(",");
            index++;
        }
        
        foreach (var translatableField in translatableFields2)
        {
            if (translatableField.HasForeignKeyTable())
            {
                sb.Append(", ");
                break;
            }
        }
        
        int index03 = 0;
        foreach (var translatableField in translatableFields2)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                index03++;
                continue;
            }
            var field = translatableField.QueryName();
            var table = translatableField.ForeignKeyTable().QueryName();
            var translationsTable = translatableField.ForeignKeyTable().TranslationsTableQueryName();
            if (translationsTable == null)
            {
                throw new ReflectionException($"Translations table not found for column {field} in table {table}");
            }
            sb.Append("jsonb_agg");
            sb.Append("(");
            sb.Append("jsonb_build_object");
            sb.Append("(");
            sb.Append("'language_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.language_id");
            sb.Append(",");
            sb.Append("'content_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content_id");
            sb.Append(",");
            sb.Append("'content'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content");
            sb.Append(")");
            sb.Append(")");
            sb.Append(" ");
            sb.Append("as");
            sb.Append(" ");
            sb.Append($"{translatableField.TranslationsQueryName()}");
            if (index03 < translatableFields2.Count - 1)
                sb.Append(",");
            index03++;
        }
        sb.Append(" ");
        
        sb.Append($"FROM {temporaryTableName}");
        
        // Additional tables
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
            sb.Append($"{column.ForeignKeyTable().QueryName()} as {GetTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{temporaryTableName}.{column.QueryName()}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetTableSynonym(column)}.{columnReflection.QueryName()}");
        }
        
        sb.Append($" ");
        
        // Additional tables 2
        foreach (var column in additionalTables2)
        {
            // LEFT JOIN attribute_types as attribute_types_3 ON product_families_attributes.attribute_type = attributes_1.attribute
            // LEFT JOIN attribute_types as attribute_types_3 ON attributes_1.attribute_type = attribute_types.id
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{column.AdditionalForeignKeyTable().QueryName()} as {GetAdditionalJoinTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{GetTableSynonym(column.ForeignKeyColumn())}.{column.QueryName()}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetAdditionalJoinTableSynonym(column)}.{column.AdditionalForeignKeyColumnName()}");
        }
        
        sb.Append($" ");
        
        foreach (var column in allTranslationJoins)
        {
            var translationsTable = column.ForeignKeyTable().TranslationsTableQueryName();
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{translationsTable} as {GetTranslationsTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{GetTableSynonym(column)}.{column.QueryName()}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetTranslationsTableSynonym(column)}.content_id");
        }
        
        sb.Append($" ");
        
        foreach (var column in allTranslationJoins2)
        {
            var field = column.QueryName();
            var table = column.AdditionalForeignKeyColumn().AdditionalForeignKeyTable().QueryName();
            var translationsTable = column.AdditionalForeignKeyColumn().AdditionalForeignKeyTable().TranslationsTableQueryName();
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{translationsTable} as {GetTranslationsTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{GetAdditionalJoinTableSynonym(column.AdditionalForeignKeyColumn())}.{field}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetTranslationsTableSynonym(column)}.content_id");
        }
        sb.Append($" ");

        
        if (allTranslationJoins.Count > 0)
        {
            sb.Append(" ");
            sb.Append("GROUP BY");
            sb.Append(" ");
            var connectedTableColumns = _reflection.MainTableReflection.Columns().ToList();
            for (int i = 0; i < connectedTableColumns.Count; i++)
            {
                var column = connectedTableColumns[i];
                sb.Append($"{temporaryTableName}.{column.QueryName()}");
                if (i < connectedTableColumns.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
            }

            if (additionalColumns.Count > 0)
            {
                sb.Append(",");
                sb.Append(" ");
            }
            
            for (int i = 0; i < additionalColumns.Count; i++)
            {
                var column = additionalColumns[i];
                ITableReflection foreignKeyTable = column.ForeignKeyTable();
                sb.Append($"{GetTableSynonym(column)}.{column.QueryName()}");
                if (i < additionalColumns.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
            }
            
            if (additionalColumns2.Count > 0)
            {
                sb.Append(",");
                sb.Append(" ");
            }
            
            int i0 = 0;
            foreach (var column in additionalColumns2)
            {
                ITableReflection foreignKeyTable = column.ForeignKeyTable();
                sb.Append($"{GetAdditionalTableSynonym(column)}.{column.QueryName()}");
                if (i0 < additionalColumns2.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
                i0++;
            }
            
        }
        
        if (_sorting.HasMainTableSortingElements())
        {
            sb.Append(" ");
            sb.Append("ORDER BY");
            sb.Append(" ");
            List<ISortingElement<T>> sortingElements = _sorting.ToList();
            for (int i = 0; i < sortingElements.Count; i++)
            {
                var sortingElement = sortingElements[i];
                var column = sortingElement.Column();
                sb.Append($"{temporaryTableName}.{column.QueryName()} {sortingElement.DirectionAsString()}");
                if (i < sortingElements.Count - 1)
                {
                    sb.Append(",");
                }
                sb.Append(" ");
            }
        }
        
        sb.Append($";");
        
        return sb.ToString();
        
    }
    
    private string ConnectedTableSelectQuery(IConnectedTable connectedTable, string tableName)
    {
        
        var additionalTables = connectedTable.TableReflection().ColumnsWithForeignKey().ToList();
        var additionalTables2 = connectedTable.TableReflection().AdditionalFieldsWithForeignKey().ToList();
        var additionalColumns = connectedTable.TableReflection().AdditionalFields().ToList();
        var additionalColumns2 = connectedTable.TableReflection().AdditionalFields2().ToList();
        
        List<IColumnReflection> allTranslationJoins = new List<IColumnReflection>();
        var translatableFields = connectedTable.TableReflection().TranslatableColumns().ToList();
        foreach (var translatableField in translatableFields)
        {
            if (translatableField.HasForeignKeyTable())
            {
                allTranslationJoins.Add(translatableField);
            }
        }
        
        List<IColumnReflection> allTranslationJoins2 = new List<IColumnReflection>();
        var translatableFields2 = connectedTable.TableReflection().AdditionalTranslatableColumns().ToList();
        foreach (var translatableField in translatableFields2)
        {
            if (translatableField.HasForeignKeyTable())
            {
                allTranslationJoins2.Add(translatableField);
            }
        }
        
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
        if (additionalColumns.Count > 0)
            sb.Append(",");

        int index0 = 0;
        foreach (var column in additionalColumns)
        {
            ITableReflection foreignKeyTable = column.ForeignKeyTable();
            sb.Append($"{GetTableSynonym(column)}.{column.QueryName()} as {foreignKeyTable.QueryName()}_{column.QueryName()}");
            if (index0 < additionalColumns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
            index0++;
        }
        
        // additional fields 2
        if (additionalColumns2.Count > 0)
            sb.Append(",");
        
        int index1 = 0;
        foreach (var column in additionalColumns2)
        {
            ITableReflection foreignKeyTable = column.ForeignKeyTable();
            sb.Append($"{GetAdditionalTableSynonym(column)}.{column.QueryName()} as {foreignKeyTable.QueryName()}_{column.QueryName()}");
            if (index1 < additionalColumns2.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
            index1++;
        }
        
        foreach (var translatableField in translatableFields)
        {
            if (translatableField.HasForeignKeyTable())
            {
                sb.Append(", ");
                break;
            }
        }
        
        int index = 0;
        bool atLeastOneTranslationJoin = false;
        foreach (var translatableField in translatableFields)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                index++;
                continue;
            }
            atLeastOneTranslationJoin = true;
            var field = translatableField.QueryName();
            var table = translatableField.ForeignKeyTable().QueryName();
            var translationsTable = translatableField.ForeignKeyTable().TranslationsTableQueryName();
            if (translationsTable == null)
            {
                throw new ReflectionException($"Translations table not found for column {field} in table {table}");
            }
            sb.Append("jsonb_agg");
            sb.Append("(");
            sb.Append("jsonb_build_object");
            sb.Append("(");
            sb.Append("'language_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.language_id");
            sb.Append(",");
            sb.Append("'content_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content_id");
            sb.Append(",");
            sb.Append("'content'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content");
            sb.Append(")");
            sb.Append(")");
            sb.Append(" ");
            sb.Append("as");
            sb.Append(" ");
            sb.Append($"{translatableField.TranslationsQueryName()}");
            if (index < translatableFields.Count - 1)
                sb.Append(",");
            index++;
        }
        sb.Append(" ");
        
        foreach (var translatableField in translatableFields2)
        {
            if (translatableField.HasForeignKeyTable())
            {
                sb.Append(", ");
                break;
            }
        }
        
        int index03 = 0;
        foreach (var translatableField in translatableFields2)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                index03++;
                continue;
            }
            atLeastOneTranslationJoin = true;
            var field = translatableField.QueryName();
            var table = translatableField.ForeignKeyTable().QueryName();
            var translationsTable = translatableField.ForeignKeyTable().TranslationsTableQueryName();
            if (translationsTable == null)
            {
                throw new ReflectionException($"Translations table not found for column {field} in table {table}");
            }
            sb.Append("jsonb_agg");
            sb.Append("(");
            sb.Append("jsonb_build_object");
            sb.Append("(");
            sb.Append("'language_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.language_id");
            sb.Append(",");
            sb.Append("'content_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content_id");
            sb.Append(",");
            sb.Append("'content'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content");
            sb.Append(")");
            sb.Append(")");
            sb.Append(" ");
            sb.Append("as");
            sb.Append(" ");
            sb.Append($"{translatableField.TranslationsQueryName()}");
            if (index03 < translatableFields2.Count - 1)
                sb.Append(",");
            index03++;
        }
        sb.Append(" ");
        
        sb.Append($"FROM {connectedTable.QueryName()} as {connectedTable.QueryName()}");
        sb.Append($" ");
        // Additional tables
        foreach (var column in additionalTables)
        {
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{column.ForeignKeyTable().QueryName()} as {GetTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{connectedTable.QueryName()}.{column.QueryName()}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetTableSynonym(column)}.{column.ForeignKeyColumn().QueryName()}");
        }
        sb.Append($" ");
        // Additional tables 2
        foreach (var column in additionalTables2)
        {
            // LEFT JOIN attribute_types as attribute_types_3 ON product_families_attributes.attribute_type = attributes_1.attribute
            // LEFT JOIN attribute_types as attribute_types_3 ON attributes_1.attribute_type = attribute_types.id
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{column.AdditionalForeignKeyTable().QueryName()} as {GetAdditionalJoinTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{GetTableSynonym(column.ForeignKeyColumn())}.{column.QueryName()}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetAdditionalJoinTableSynonym(column)}.{column.AdditionalForeignKeyColumnName()}");
        }
        
        sb.Append($" ");
        
        foreach (var column in allTranslationJoins)
        {
            var field = column.QueryName();
            var table = column.ForeignKeyTable().QueryName();
            var translationsTable = column.ForeignKeyTable().TranslationsTableQueryName();
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{translationsTable} as {GetTranslationsTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{GetTableSynonym(column)}.{field}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetTranslationsTableSynonym(column)}.content_id");
        }
        sb.Append($" ");
        
        foreach (var column in allTranslationJoins2)
        {
            var field = column.QueryName();
            var table = column.AdditionalForeignKeyColumn().AdditionalForeignKeyTable().QueryName();
            var translationsTable = column.AdditionalForeignKeyColumn().AdditionalForeignKeyTable().TranslationsTableQueryName();
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{translationsTable} as {GetTranslationsTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{GetAdditionalJoinTableSynonym(column.AdditionalForeignKeyColumn())}.{field}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetTranslationsTableSynonym(column)}.content_id");
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
        if (atLeastOneTranslationJoin)
        {
            sb.Append(" ");
            sb.Append("GROUP BY");
            sb.Append(" ");
            int index200 = 0;
            for (int i = 0; i < connectedTableColumns.Count; i++)
            {
                var column = connectedTableColumns[i];
                sb.Append($"{connectedTable.TableReflection().QueryName()}.{column.QueryName()}");
                if (index200 < connectedTableColumns.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
                index200++;
            }

            if (additionalColumns.Count > 0)
            {
                sb.Append(",");
                sb.Append(" ");
            }
            
            index200 = 0;
            foreach (var column in additionalColumns)
            {
                ITableReflection foreignKeyTable = column.ForeignKeyTable();
                sb.Append($"{GetTableSynonym(column)}.{column.QueryName()}");
                if (index200 < additionalColumns.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
                index200++;
            }
            
            if (additionalColumns2.Count > 0)
            {
                sb.Append(",");
                sb.Append(" ");
            }
            
            index200 = 0;
            foreach (var column in additionalColumns2)
            {
                ITableReflection foreignKeyTable = column.ForeignKeyTable();
                sb.Append($"{GetAdditionalTableSynonym(column)}.{column.QueryName()}");
                if (index200 < additionalColumns2.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
                index200++;
            }
            
        }
        sb.Append(";");
        
        return sb.ToString();
        
    }
    
    private void FillReplacements(ITableReflection tableReflection)
    {
        _tableReplacements.Clear();
        _translationsTableReplacements.Clear();
        var additionalColumns = tableReflection.ColumnsWithForeignKey().ToList();
        List<String[]> allTranslationJoins = new List<String[]>();
        var translatableFields = tableReflection.TranslatableColumns().ToList();
        foreach (var translatableField in translatableFields)
        {
            if (translatableField.HasForeignKeyTable())
            {
                var field = translatableField.QueryName();
                var table = translatableField.ForeignKeyTable().QueryName();
                var translationsTable = translatableField.ForeignKeyTable().TranslationsTableQueryName();
                allTranslationJoins.Add([translationsTable, table, field]);
            }
        }
        
        var translatableFields2 = tableReflection.AdditionalTranslatableColumns().ToList();
        foreach (var translatableField in translatableFields2)
        {
            if (translatableField.HasForeignKeyTable())
            {
                var field = translatableField.QueryName();
                var table = translatableField.ForeignKeyTable().QueryName();
                var translationsTable = translatableField.ForeignKeyTable().TranslationsTableQueryName();
                allTranslationJoins.Add([translationsTable, table, field]);
            }
        }

        int a = 1;
        foreach (var column in additionalColumns)
        {
            string field = column.QueryName();
            string table = column.Table().QueryName();
            string foreignKeyName = column.ForeignKeyName();
            string searchField = $"{foreignKeyName}.{field}";
            _tableReplacements.Add(searchField, $"{column.ForeignKeyTable().QueryName()}_{a}");
            a++;
        }
        
        var additionalColumns2 = tableReflection.AdditionalFieldsWithForeignKey().ToList();
        foreach (var column in additionalColumns2)
        {
            string field = column.QueryName();
            string table = column.ForeignKeyTable().QueryName();
            string foreignKeyName = column.ForeignKeyName();
            string searchField = $"{foreignKeyName}.{field}";
            _tableReplacements.Add(searchField, $"{column.AdditionalForeignKeyTable().QueryName()}_{a}");
            a++;
        }
        
        foreach (var column in translatableFields)
        {
            if (column.HasForeignKeyTable())
            {
                string field = column.QueryName();
                string table = column.ForeignKeyTable().QueryName();
                string foreignKeyName = column.ForeignKeyName();
                string searchField = $"{foreignKeyName}.{field}";
                _tableReplacements.Add(searchField, $"{column.ForeignKeyTable().QueryName()}_{a}");
                _translationsTableReplacements.Add(searchField, $"{column.TranslationsQueryName()}_{a}");
                a++;
            }
        }
        
        foreach (var column in translatableFields2)
        {
            if (column.HasForeignKeyTable())
            {
                string field = column.QueryName();
                string table = column.ForeignKeyTable().QueryName();
                string foreignKeyName = column.ForeignKeyName();
                string searchField = $"{foreignKeyName}.{field}";
                _tableReplacements.Add(searchField, $"{column.ForeignKeyTable().QueryName()}_{a}");
                _translationsTableReplacements.Add(searchField, $"{column.TranslationsQueryName()}_{a}");
                a++;
            }
        }
    }

    private string GetTableSynonym(IColumnReflection column)
    {
        string field = column.QueryName();
        if (column.HasForeignKeyTable())
        {
            IColumnReflection foreignKeyColumn = column.Table().GetForeignKeyColumn(column.ForeignKeyName());
            field = foreignKeyColumn.QueryName();
        }
        
        string table = column.Table().QueryName();
        string queryName = column.ForeignKeyTable().QueryName();
        string foreignKeyName = column.ForeignKeyName();
        string searchField = $"{foreignKeyName}.{field}";
        if (_tableReplacements.ContainsKey(searchField))
        {
            queryName = _tableReplacements[searchField];
        }
        return queryName;
    }
    
    private string GetAdditionalTableSynonym(IColumnReflection column)
    {
        string field = column.AdditionalForeignKeyColumn().QueryName();
        string table = column.ForeignKeyTable().QueryName();
        string queryName = column.ForeignKeyTable().QueryName();
        string foreignKeyName = column.ForeignKeyName();
        string searchField = $"{foreignKeyName}.{field}";
        if (_tableReplacements.ContainsKey(searchField))
        {
            queryName = _tableReplacements[searchField];
        }
        return queryName;
    }
    
    private string GetAdditionalJoinTableSynonym(IColumnReflection column)
    {
        string field = column.QueryName();
        string table = column.ForeignKeyTable().QueryName();
        string queryName = column.ForeignKeyTable().QueryName();
        string foreignKeyName = column.ForeignKeyName();
        string searchField = $"{foreignKeyName}.{field}";
        if (_tableReplacements.ContainsKey(searchField))
        {
            queryName = _tableReplacements[searchField];
        }
        return queryName;
    }
    
    private string GetTranslationsTableSynonym(IColumnReflection column)
    {
        string field = column.QueryName();
        string table = column.ForeignKeyTable().QueryName();
        string queryName = column.TranslationsQueryName();
        string foreignKeyName = column.ForeignKeyName();
        string searchField = $"{foreignKeyName}.{field}";
        if (_translationsTableReplacements.ContainsKey(searchField))
        {
            queryName = _translationsTableReplacements[searchField];
        }
        return queryName;
    }
    
}