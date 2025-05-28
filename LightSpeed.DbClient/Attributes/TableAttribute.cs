namespace LightSpeed.DbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class TableAttribute : Attribute
{
    public readonly string? Name;

    public TableAttribute()
    {
    }
    
    public TableAttribute(string name)
    {
        Name = name;
    }
}