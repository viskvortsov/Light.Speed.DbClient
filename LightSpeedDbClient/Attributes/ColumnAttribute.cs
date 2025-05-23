namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute : Attribute
{
    public readonly string? Name;

    public ColumnAttribute()
    {
    }
    
    public ColumnAttribute(string name)
    {
        Name = name;
    }
    
}