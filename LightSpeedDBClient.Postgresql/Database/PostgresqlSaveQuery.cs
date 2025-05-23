using System.Text;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlSaveQuery<E>: IQuery where E : IDatabaseObject
{
    
    private readonly DatabaseObjectReflection _reflection;
    private readonly QueryParameters _parameters;
    private readonly List<E> _elements;
    private int _paramsCount;

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
        _paramsCount = 0;
        
        StringBuilder sb = new StringBuilder();
        sb.Append(MainRowInsertQuery());
        
        List<IConnectedTable> connectedTables = _reflection.ConnectedTables().ToList();
        foreach (var connectedTable in connectedTables)
        {
            sb.Append(ConnectedTableDeleteQuery(connectedTable));
            sb.Append(ConnectedTableInsertQuery(connectedTable));
        }
        
        return sb.ToString();
    }

    public IQueryParameters Parameters()
    {
        return _parameters;
    }
    
    private string MainRowInsertQuery()
    {
        
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
                string parameterName = _parameters.Add(type, value);
                parameterNamesBuilder.Append(parameterName);
                if (i < columns.Count - 1)
                    parameterNamesBuilder.Append(",");
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
        
        sb.Append(";");
        sb.Append("\n");
        
        return sb.ToString();
        
    }

    private string ConnectedTableDeleteQuery(IConnectedTable connectedTable)
    {
        
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
                IColumnReflection ownerColumn = _reflection.MainTableReflection.GetColumnReflection(keyPart.Relation());
                var value = ownerColumn.Property().GetValue(element);
                string parameterName = _parameters.Add(ownerColumn.Type(), value);
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
        return sb.ToString();
    }

    private string ConnectedTableInsertQuery(IConnectedTable connectedTable)
    {
        
        StringBuilder sb = new StringBuilder();

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

        if (!atLeastOneElementHaveRowInTable)
        {
            return sb.ToString();
        }
        
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
                    string parameterName = _parameters.Add(column.Type(), column.Property().GetValue(row));
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
        
        return sb.ToString();
    }
    
}