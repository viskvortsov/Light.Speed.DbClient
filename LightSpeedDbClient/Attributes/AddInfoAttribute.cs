namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class AddInfoAttribute : Attribute
{
    
    public readonly string ForeignKey;
    public readonly string Field;
    
    public AddInfoAttribute(string foreignKey, string field)
    {
        ForeignKey = foreignKey;
        Field = field;
    }
}