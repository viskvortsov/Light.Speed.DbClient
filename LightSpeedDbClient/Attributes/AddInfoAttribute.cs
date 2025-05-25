namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class AddInfoAttribute(string foreignKey, string field) : Attribute
{
    public readonly string ForeignKey = foreignKey;
    public readonly string Field = field;
}