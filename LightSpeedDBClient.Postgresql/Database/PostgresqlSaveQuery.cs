using System.Text;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlSaveQuery<E>: IQuery where E : IDatabaseElement
{
    
    private readonly DatabaseObjectReflection _reflection;
    private readonly QueryParameters _parameters;
    private readonly List<E> _elements;

    public PostgresqlSaveQuery(DatabaseObjectReflection reflection, E element) : this(reflection, new List<E> { element }) {}
    
    public PostgresqlSaveQuery(DatabaseObjectReflection reflection, IEnumerable<E> elements)
    {
        _reflection = reflection;
        _elements = elements.ToList();
        _parameters = new ();
    }

    public string GetQueryText()
    {
        
        _parameters.Clear();
        
        StringBuilder columnNamesBuilder = new StringBuilder();
        StringBuilder parameterNamesBuilder = new StringBuilder();
        StringBuilder keyBuilder = new StringBuilder();
        
        var columns = _reflection.MainTableReflection.Columns().ToList();
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            columnNamesBuilder.Append($"{column.QueryName()}");
            if (i < columns.Count - 1)
                columnNamesBuilder.Append(",");
        }

        int a = 1;
        for (int l = 0; l < _elements.Count; l++)
        {
            
            var element = _elements[l];
            
            parameterNamesBuilder.Append("(");

            for (int i = 0; i < columns.Count; i++)
            {

                var column = columns[i];
                var property = column.Property();
                var value = property.GetValue(element);
                var type = property.PropertyType;
                string parameterName = $"@{a}";
                
                _parameters.Add(new QueryParameter(parameterName, type, value));
                
                parameterNamesBuilder.Append(parameterName);
                if (i < columns.Count - 1)
                    parameterNamesBuilder.Append(",");
                a++;

            }
            
            parameterNamesBuilder.Append(")");
            if (l < _elements.Count - 1)
                parameterNamesBuilder.Append(",");

        }
        
        var partsOfPrimaryKey = _reflection.MainTableReflection.PartsOfPrimaryKey().ToList();
        for (int i = 0; i < partsOfPrimaryKey.Count; i++)
        {
            var column = partsOfPrimaryKey[i];
            keyBuilder.Append($"{column.QueryName()}");
            if (i < partsOfPrimaryKey.Count - 1)
                keyBuilder.Append(",");
        }

        StringBuilder sb = new StringBuilder();
        sb.Append($"INSERT INTO");
        sb.Append($" ");
        sb.Append($"{_reflection.MainTableReflection.QueryName()}({columnNamesBuilder.ToString()})");
        sb.Append($" ");
        sb.Append($"VALUES {parameterNamesBuilder.ToString()}");
        sb.Append($" ");
        sb.Append($"ON CONFLICT ({keyBuilder.ToString()}) DO UPDATE SET");
        sb.Append($" ");
        
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            sb.Append($"{column.QueryName()} = EXCLUDED.{column.QueryName()}");
            if (i < columns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        
        sb.Append($"RETURNING");
        sb.Append(" ");
        sb.Append($"{columnNamesBuilder.ToString()}");
            
        return sb.ToString();
        
    }

    public IQueryParameters Parameters()
    {
        return _parameters;
    }
    
}