namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class OwnerKeyAttribute : Attribute
{
    
    public readonly string Relation;

    public OwnerKeyAttribute(string relation)
    {
        Relation = relation;
    }
    
}