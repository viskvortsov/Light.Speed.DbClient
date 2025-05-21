using System.Reflection;

namespace LightSpeedDbClient.Reflections;

public interface IColumnReflection
{
    string Name();
    string QueryName();
    Type Type();
    string Relation();
    PropertyInfo Property();
    bool IsPartOfPrimaryKey();
    bool IsPartOfOwnerKey();
}