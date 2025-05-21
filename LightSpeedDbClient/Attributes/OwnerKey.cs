namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class OwnerKeyAttribute : Attribute
{
    
    public string Relation;

    public OwnerKeyAttribute(string relation)
    {
        Relation = relation;
    }
    
}