namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyAttribute : Attribute
{
    public readonly string Name;
    public readonly Type Model;
    public readonly string ColumnName;
    
    public ForeignKeyAttribute(string name, Type model, string columnName)
    {
        Name = name;
        Model = model;
        ColumnName = columnName;
    }
}