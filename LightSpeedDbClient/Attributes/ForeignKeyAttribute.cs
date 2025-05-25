namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyAttribute(string name, Type model, string columnName) : Attribute
{
    public readonly string Name = name;
    public readonly Type Model = model;
    public readonly string ColumnName = columnName;
}