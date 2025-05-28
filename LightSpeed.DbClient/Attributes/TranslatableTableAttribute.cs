namespace LightSpeed.DbClient.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TranslatableTableAttribute(string table) : Attribute
{
    public readonly string Table = table;
}