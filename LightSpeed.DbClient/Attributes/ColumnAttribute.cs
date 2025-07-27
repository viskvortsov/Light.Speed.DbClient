namespace LightSpeed.DbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute : Attribute
{
    public readonly string? Name;
    public readonly bool ReadOnly = false;

    public ColumnAttribute()
    {
    }
    
    public ColumnAttribute(string name, bool readOnly = false)
    {
        Name = name;
        ReadOnly = readOnly;
    }
    
    public ColumnAttribute(bool readOnly)
    {
        ReadOnly = readOnly;
    }
    
}