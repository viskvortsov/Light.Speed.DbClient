namespace LightSpeed.DbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class AddInfoAdditionalAttribute(string foreignKey, string field) : Attribute
{
    public readonly string ForeignKey = foreignKey;
    public readonly string Field = field;
}