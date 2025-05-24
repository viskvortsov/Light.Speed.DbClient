using System.Text;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlSelectByKeyQuery: IQuery
{
    
    private readonly DatabaseObjectReflection _reflection;
    private readonly QueryParameters _parameters;
    private readonly IKey _key;
    
    public PostgresqlSelectByKeyQuery(DatabaseObjectReflection reflection, IKey key)
    {
        _reflection = reflection;
        _key = key;
        _parameters = new ();
    }

    public string GetQueryText()
    {
        _parameters.Clear();
        StringBuilder sb = new StringBuilder();
        sb.Append(MainRowSelectQuery());
        sb.Append(" ");
        
        List<IConnectedTable> connectedTables = _reflection.ConnectedTables().ToList();
        foreach (var connectedTable in connectedTables)
        {
            sb.Append(ConnectedTableSelectQuery(connectedTable));
            sb.Append(" ");
        }
        
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
        
        sb.Append($" ");
        sb.Append($"WHERE");
        sb.Append($" ");
        List<IKeyElement> keyElements = _key.KeyElements().ToList();
        for (int i = 0; i < keyElements.Count; i++)
        {
            var keyPart = keyElements[i];
            var value = keyPart.Value();
            var type = value.GetType();
            string parameterName = _parameters.Add(type, value);
            sb.Append($"{_reflection.MainTableReflection.QueryName()}.{keyPart.Column().QueryName()} = {parameterName}");
            if (i < keyElements.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        sb.Append(";");
        return sb.ToString();
    }
    
    private string ConnectedTableSelectQuery(IConnectedTable connectedTable)
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
        var columnsWithForeignKey = _reflection.MainTableReflection.ColumnsWithForeignKey();
        Dictionary<IColumnReflection, IColumnReflection> allAdditionalFields = new ();
        var withForeignKey = columnsWithForeignKey.ToList();
        foreach (var column in withForeignKey)
        {
            foreach (IColumnReflection additionalField in _reflection.MainTableReflection.ColumnsWithAdditionalInfo(column.ForeignKeyName()))
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
        sb.Append($"FROM {connectedTable.QueryName()} as {connectedTable.QueryName()}");
        
        // Additional tables
        foreach (var column in withForeignKey)
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
        
        sb.Append($" ");
        sb.Append($"WHERE");
        sb.Append($" ");
            
        List<IColumnReflection> ownerKeys = connectedTable.TableReflection().PartsOfOwnerKey().ToList();
        for (int i = 0; i < ownerKeys.Count; i++)
        {
            var keyPart = ownerKeys[i];
            IKeyElement partOfKey = _key.KeyElements().First(element => element.Column().Name() == keyPart.Relation());
            var value = partOfKey.Value();
            var type = value.GetType();
            string parameterName =_parameters.Add(type, value);
            sb.Append($"{connectedTable.QueryName()}.{keyPart.QueryName()} = {parameterName}");
            if (i < ownerKeys.Count - 1)
            {
                sb.Append(" ");
                sb.Append("AND");
            }
            sb.Append(" ");
        }
        sb.Append(";");
        return sb.ToString();
    }
    
}