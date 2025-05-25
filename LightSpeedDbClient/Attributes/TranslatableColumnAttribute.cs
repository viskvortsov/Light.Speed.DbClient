namespace LightSpeedDbClient.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class TranslatableColumnAttribute : Attribute
{
    public TranslatableColumnAttribute()
    {
    }
    
}