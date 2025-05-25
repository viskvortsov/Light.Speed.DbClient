namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ModelAttribute(string table, int id = 0) : Attribute
{
    public readonly string Table = table;
    public readonly int Id = id;
}