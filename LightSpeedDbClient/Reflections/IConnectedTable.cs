using System.Reflection;

namespace LightSpeedDbClient.Reflections;

public interface IConnectedTable
{
    string Name();
    string QueryName();
    ITableReflection TableReflection();
    Type Type();
    PropertyInfo Property();

}