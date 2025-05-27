using System.Text;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlSaveManyQueries<T>(DatabaseObjectReflection reflection, IEnumerable<T> elements, IMapper mapper)
    where T : IDatabaseObject
{
    private readonly List<T> _elements = elements.ToList();
    private readonly List<PostgresqlSubQuery> _subQueries = new ();

    public PostgresqlSaveManyQueries(DatabaseObjectReflection reflection, T element, PostgresqlMapper mapper) : this(reflection, new List<T> { element }, mapper) {}

    public List<PostgresqlSubQuery> GetQueries()
    {

        _subQueries.Add(MainRowInsertQuery());
        
        List<IConnectedTable> connectedTables = reflection.ConnectedTables().ToList();
        foreach (var connectedTable in connectedTables)
        {
            _subQueries.Add(ConnectedTableDeleteQuery(connectedTable));
            
            bool atLeastOneElementHaveRowInTable = false;
            foreach (var element in _elements)
            {
                IDatabaseObjectTable table = element.Table(connectedTable.Name());
                if (table.Count > 0)
                {
                    atLeastOneElementHaveRowInTable = true;
                    break;
                }
            }

            if (atLeastOneElementHaveRowInTable)
            {
                _subQueries.Add(ConnectedTableInsertQuery(connectedTable));
            }
            
        }
        
        List<IConnectedTable> translationTables = reflection.TranslationTables().ToList();
        foreach (var connectedTable in translationTables)
        {
            _subQueries.Add(ConnectedTableDeleteQuery(connectedTable));
            
            bool atLeastOneElementHaveRowInTable = false;
            foreach (var element in _elements)
            {
                IDatabaseObjectTable table = element.Table(connectedTable.Name());
                if (table.Count > 0)
                {
                    atLeastOneElementHaveRowInTable = true;
                    break;
                }
            }

            if (atLeastOneElementHaveRowInTable)
            {
                _subQueries.Add(ConnectedTableInsertQuery(connectedTable));
            }
            
        }
        
        return _subQueries;
    }
    
    private PostgresqlSubQuery MainRowInsertQuery()
    {
        
        QueryParameters parameters = new QueryParameters();
        StringBuilder columnNamesBuilder = new StringBuilder();
        StringBuilder parameterNamesBuilder = new StringBuilder();
        StringBuilder keyBuilder = new StringBuilder();
        
        var columns = reflection.MainTableReflection.Columns().ToList();
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            columnNamesBuilder.Append($"{column.QueryName()}");
            if (i < columns.Count - 1)
                columnNamesBuilder.Append(",");
        }
        
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
                value = mapper.MapToDatabaseValue(value, type);
                string parameterName = parameters.Add(type, value);
                parameterNamesBuilder.Append(parameterName);
                if (i < columns.Count - 1)
                    parameterNamesBuilder.Append(",");
            }
            
            parameterNamesBuilder.Append(")");
            if (l < _elements.Count - 1)
                parameterNamesBuilder.Append(",");

        }
        
        var partsOfPrimaryKey = reflection.MainTableReflection.PartsOfPrimaryKey().ToList();
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
        sb.Append($"{reflection.MainTableReflection.QueryName()}({columnNamesBuilder.ToString()})");
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
        
        sb.Append(";");
        sb.Append("\n");
        
        return new PostgresqlSubQuery(sb.ToString(), parameters);
        
    }

    private PostgresqlSubQuery ConnectedTableDeleteQuery(IConnectedTable connectedTable)
    {
        
        QueryParameters parameters = new QueryParameters();
        StringBuilder keyBuilder = new StringBuilder();
        
        var partsOfOwnerKey = connectedTable.TableReflection().PartsOfOwnerKey().ToList();
        for (int i = 0; i < partsOfOwnerKey.Count; i++)
        {
            var column = partsOfOwnerKey[i];
            keyBuilder.Append($"{column.QueryName()}");
            if (i < partsOfOwnerKey.Count - 1)
                keyBuilder.Append(",");
        }
        
        StringBuilder sb = new StringBuilder();
            
        sb.Append($"DELETE");
        sb.Append($" ");
        sb.Append($"FROM {connectedTable.QueryName()}");
        sb.Append($" ");
        sb.Append($"WHERE");
        sb.Append($" ");

        int index0 = 0;
        sb.Append($"(");
        foreach (IColumnReflection keyPart in partsOfOwnerKey)
        {
            sb.Append($"{connectedTable.QueryName()}.{keyPart.QueryName()}");
            if (index0 < partsOfOwnerKey.Count - 1)
                sb.Append(", ");
        }
        sb.Append($")");
        sb.Append($" ");
        sb.Append($"IN");
        sb.Append($" ");
        sb.Append($"(");
        int index1 = 0;
        foreach (var element in _elements)
        {
            int index2 = 0;
            sb.Append($"(");
            foreach (IColumnReflection keyPart in partsOfOwnerKey)
            {

                string? relation = keyPart.Relation();
                if (relation == null)
                    throw new ModelSetupException($"Relation is null for column {keyPart.QueryName()}");
                IColumnReflection? ownerColumn = reflection.MainTableReflection.GetColumnReflection(relation);
                if (ownerColumn == null)
                    throw new ModelSetupException($"Owner column is null for column {keyPart.QueryName()}");
                object? value = ownerColumn.Property().GetValue(element);
                Type type = ownerColumn.Type();
                value = mapper.MapToDatabaseValue(value, type);
                string parameterName = parameters.Add(ownerColumn.Type(), value);
                sb.Append($"{parameterName}");
                if (index2 < partsOfOwnerKey.Count - 1)
                    sb.Append(", ");
                index2++;
            }
            sb.Append($")");
            if (index1 < _elements.Count - 1)
                sb.Append(",");
            index1++;
        }
        sb.Append($")");
        sb.Append($";");
        sb.Append("\n");
        return new PostgresqlSubQuery(sb.ToString(), parameters);
    }

    private PostgresqlSubQuery ConnectedTableInsertQuery(IConnectedTable connectedTable)
    {

        QueryParameters parameters = new QueryParameters();
        StringBuilder sb = new StringBuilder();
        
        sb.Append($"INSERT INTO");
        sb.Append($" ");
        sb.Append($"{connectedTable.QueryName()}");
        sb.Append($"(");
        var connectedTableColumns = connectedTable.TableReflection().Columns().ToList();
        int index1 = 0;
        foreach (var column in connectedTableColumns)
        {
            sb.Append($"{column.QueryName()}");
            if (index1 < connectedTableColumns.Count - 1)
                sb.Append(",");
            index1++;
        }
        sb.Append($")");
        sb.Append($" ");
        sb.Append($"VALUES");
        sb.Append($" ");
        
        int index4 = 0;
        foreach (var element in _elements)
        {
            int index2 = 0;
            var table = element.Table(connectedTable.Name());
            foreach (object row in table)
            {
                int index3 = 0;
                sb.Append($"(");
                foreach (var column in connectedTableColumns)
                {
                    Type type = column.Type();
                    object? value = column.Property().GetValue(row);
                    value = mapper.MapToDatabaseValue(value, type);
                    string parameterName = parameters.Add(type, value);
                    sb.Append($"{parameterName}");
                    if (index3 < connectedTableColumns.Count - 1)
                        sb.Append(",");
                    index3++;
                }
                sb.Append($")");
                if (index2 < table.Count - 1)
                    sb.Append(",");
                index2++;
            }
            if (index4 < _elements.Count - 1)
                sb.Append(",");
            index4++;
        }
        sb.Append($" ");
        sb.Append($"RETURNING");
        sb.Append(" ");
        int index10 = 0;
        foreach (var column in connectedTableColumns)
        {
            sb.Append($"{column.QueryName()}");
            if (index10 < connectedTableColumns.Count - 1)
                sb.Append(",");
            index10++;
        }
        sb.Append(";");
        sb.Append("\n");
        
        return new PostgresqlSubQuery(sb.ToString(), parameters);
    }
    
}