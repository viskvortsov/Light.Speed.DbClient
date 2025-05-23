namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyAttribute : Attribute
{
    public readonly Type Model;
    public readonly string ColumnName;
    
    public ForeignKeyAttribute(Type model, string columnName)
    {
        Model = model;
        ColumnName = columnName;
    }
}