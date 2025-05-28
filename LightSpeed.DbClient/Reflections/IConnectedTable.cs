using System.Reflection;

namespace LightSpeed.DbClient.Reflections;

public interface IConnectedTable
{
    string Name();
    string QueryName();
    ITableReflection TableReflection();
    Type Type();
    PropertyInfo Property();

}