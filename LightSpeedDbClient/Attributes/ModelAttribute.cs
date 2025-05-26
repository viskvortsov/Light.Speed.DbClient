namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ModelAttribute(string table) : Attribute
{
    public readonly string Table = table;
}