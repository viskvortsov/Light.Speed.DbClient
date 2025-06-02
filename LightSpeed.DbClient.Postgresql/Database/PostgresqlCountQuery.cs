using System.Text;
using LightSpeed.DbClient.Database;
using LightSpeed.DbClient.Exceptions;
using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Reflections;

namespace LightSpeed.DbClient.Postgresql.Database;

public class PostgresqlCountQuery<T>: IQuery where T : IDatabaseElement
{
    
    private readonly Dictionary<string, string> _tableReplacements = new ();
    private readonly Dictionary<string, string> _translationsTableReplacements = new ();
    private readonly DatabaseObjectReflection _reflection;
    private readonly QueryParameters _parameters;
    private readonly IFilters<T> _filters;
    private readonly IMapper _mapper;
    
    public PostgresqlCountQuery(
        IFilters<T> filters,
        DatabaseObjectReflection reflection, 
        IMapper mapper)
    {
        
        _reflection = reflection;
        _mapper = mapper;
        
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
        
        if (_filters.HasConnectedTableFilters() || _filters.HasTranslationFieldsFilters())
        {
            FillReplacements(_reflection.MainTableReflection);
            FillReplacements(_reflection.MainTableReflection);
            sb.Append(CountFilterConnectedTableQuery());
        }
        else
        {
            FillReplacements(_reflection.MainTableReflection);
            FillReplacements(_reflection.MainTableReflection);
            sb.Append(CountTableQuery(tableName));
            sb.Append(" ");
        }
        
        return sb.ToString();
        
    }
    
    private string CountFilterConnectedTableQuery()
    {
        
        StringBuilder sb = new StringBuilder();
        
        sb.Append($"SELECT");
        sb.Append($" ");
        
        var columns = _reflection.MainTableReflection.PartsOfPrimaryKey().ToList();
        // main fields
        // TODO possible issue with calculation due to rows duplications 
        sb.Append($"sum(1) as count");

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
        return sb.ToString();
        
    }
    
    private string CountTableQuery(string tableName)
    {
        
        StringBuilder sb = new StringBuilder();

        sb.Append($"SELECT");
        sb.Append($" ");
        sb.Append($"sum(1) as count");
        sb.Append($" ");
        
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
                    if (type == typeof(String))
                    {
                        string parameterName = _parameters.Add(type, ((String)value!).ToLower());
                        sb.Append($"lower({table.QueryName()}.{filter.Column().QueryName()}) {ComparisonOperatorConverter.Convert(filter.Operator())} {parameterName}");
                    }
                    else
                    {
                        string parameterName = _parameters.Add(type, value);
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
        
        sb.Append(" ");
        
        sb.Append(";");
        return sb.ToString();
    }

    public IQueryParameters Parameters()
    {
        return _parameters;
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

        int a = 1;
        foreach (var column in additionalColumns)
        {
            string field = column.QueryName();
            string table = column.Table().QueryName();
            string searchField = $"{table}.{field}";
            _tableReplacements.Add(searchField, $"{column.ForeignKeyTable().QueryName()}_{a}");
            a++;
        }
        foreach (var column in translatableFields)
        {
            if (column.HasForeignKeyTable())
            {
                string field = column.QueryName();
                string table = column.ForeignKeyTable().QueryName();
                string searchField = $"{table}.{field}";
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
        string searchField = $"{table}.{field}";
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
        string searchField = $"{table}.{field}";
        if (_translationsTableReplacements.ContainsKey(searchField))
        {
            queryName = _translationsTableReplacements[searchField];
        }
        return queryName;
    }
    
}