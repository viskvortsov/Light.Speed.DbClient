namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyAttribute : Attribute
{
    public readonly string TableName;
    public readonly string ColumnName;
    
    public ForeignKeyAttribute(string tableName, string columnName)
    {
        TableName = tableName;
        ColumnName = columnName;
    }
}