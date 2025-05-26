using System.Text;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlSelectByKeyQuery(DatabaseObjectReflection reflection, IKey key, IMapper mapper) : IQuery
{
    private readonly QueryParameters _parameters = new ();

    public string GetQueryText()
    {
        _parameters.Clear();
        StringBuilder sb = new StringBuilder();
        sb.Append(MainRowSelectQuery());
        sb.Append(" ");
        
        List<IConnectedTable> connectedTables = reflection.ConnectedTables().ToList();
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
        var columns = reflection.MainTableReflection.Columns().ToList();
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            sb.Append($"{reflection.MainTableReflection.QueryName()}.{column.QueryName()} as {column.QueryName()}");
            if (i < columns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        
        // additional fields
        var additionalFields = reflection.MainTableReflection.AdditionalFields().ToList();
        
        if (additionalFields.Count > 0)
            sb.Append(",");

        int index0 = 0;
        foreach (var additionalField in additionalFields)
        {
            ITableReflection? foreignKeyTable = additionalField.ForeignKeyTable();
            if (foreignKeyTable == null)
                throw new ReflectionException($"Foreign key table not found for {additionalField.Name()}");
            sb.Append($"{foreignKeyTable.QueryName()}.{additionalField.QueryName()} as {foreignKeyTable.QueryName()}_{additionalField.QueryName()}");
            if (index0 < additionalFields.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
            index0++;
        }
        
        // Main table
        sb.Append($"FROM {reflection.MainTableReflection.QueryName()} as {reflection.MainTableReflection.QueryName()}");
        
        // Additional tables
        var additionalTables = reflection.MainTableReflection.ColumnsWithForeignKey().ToList();
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
            sb.Append($"{reflection.MainTableReflection.QueryName()}.{column.QueryName()}");
        }
        
        sb.Append($" ");
        sb.Append($"WHERE");
        sb.Append($" ");
        List<IKeyElement> keyElements = key.KeyElements().ToList();
        for (int i = 0; i < keyElements.Count; i++)
        {
            var keyPart = keyElements[i];
            object? value = keyPart.Value();
            Type type = keyPart.Type();
            value = mapper.MapToDatabaseValue(value, type);
            string parameterName = _parameters.Add(type, value);
            sb.Append($"{reflection.MainTableReflection.QueryName()}.{keyPart.Column().QueryName()} = {parameterName}");
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
        var columnsWithForeignKey = reflection.MainTableReflection.ColumnsWithForeignKey();
        Dictionary<IColumnReflection, IColumnReflection> allAdditionalFields = new ();
        var withForeignKey = columnsWithForeignKey.ToList();
        foreach (var column in withForeignKey)
        {
            string? foreignKeyName = column.ForeignKeyName();
            if (foreignKeyName == null)
                throw new ReflectionException($"Foreign key name not found for {column.Name()}");
            foreach (IColumnReflection additionalField in reflection.MainTableReflection.ColumnsWithAdditionalInfo(foreignKeyName))
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
            ITableReflection? foreignKeyTable = column.ForeignKeyTable();
            if (foreignKeyTable == null)
                throw new ReflectionException($"Foreign key table not found for {additionalField.Name()}");
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
            ITableReflection? foreignKeyTable = column.ForeignKeyTable();
            if (foreignKeyTable == null)
                throw new ReflectionException($"Foreign key table not found for {column.ForeignKeyName()}");
            IColumnReflection? foreignKeyColumn = column.ForeignKeyColumn();
            if (foreignKeyColumn == null)
                throw new ReflectionException($"Foreign key column not found for {column.ForeignKeyName()}");
            
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{foreignKeyTable.QueryName()}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{foreignKeyTable.QueryName()}.{foreignKeyColumn.QueryName()}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{reflection.MainTableReflection.QueryName()}.{column.QueryName()}");
        }
        
        sb.Append($" ");
        sb.Append($"WHERE");
        sb.Append($" ");
            
        List<IColumnReflection> ownerKeys = connectedTable.TableReflection().PartsOfOwnerKey().ToList();
        for (int i = 0; i < ownerKeys.Count; i++)
        {
            var keyPart = ownerKeys[i];
            IKeyElement partOfKey = key.KeyElements().First(element => element.Column().Name() == keyPart.Relation());
            var value = partOfKey.Value();
            var type = partOfKey.Type();
            value = mapper.MapToDatabaseValue(value, type);
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