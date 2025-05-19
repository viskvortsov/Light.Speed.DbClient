namespace LightSpeedDbClient.Reflections;

public interface IReflector
{
    ITableReflection GetTableReflection(Type type);
}