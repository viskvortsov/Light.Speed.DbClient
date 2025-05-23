namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class AddInfoAttribute : Attribute
{
    public readonly string[] _fields;
    
    public AddInfoAttribute(string[] fields)
    {
        _fields = fields;
    }
}