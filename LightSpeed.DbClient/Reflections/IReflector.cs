namespace LightSpeed.DbClient.Reflections;

public interface IReflector
{
    ITableReflection GetTableReflection(Type type);
}