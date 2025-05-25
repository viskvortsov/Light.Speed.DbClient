namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TranslatableTableAttribute : Attribute
{
    public readonly string Table;
    
    public TranslatableTableAttribute(string table)
    {
        Table = table;
    }
    
}