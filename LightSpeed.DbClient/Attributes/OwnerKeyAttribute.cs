namespace LightSpeed.DbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class OwnerKeyAttribute(string? relation) : Attribute
{
    
    public readonly string? Relation = relation;
}