namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class TableAttribute : Attribute
{
    public string? Name;

    public TableAttribute()
    {
    }
    
    public TableAttribute(string name)
    {
        Name = name;
    }
}