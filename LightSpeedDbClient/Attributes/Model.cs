namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ModelAttribute : Attribute
{
    public string Table;

    public ModelAttribute(string table)
    {
        Table = table;
    }
    
}