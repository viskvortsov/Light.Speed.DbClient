using System.Reflection;
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Reflections;

public interface IColumnReflection
{
    string Name();
    string QueryName();
    Type Type();
    IQueryType QueryType();
    string Relation();
    PropertyInfo Property();
    bool IsPartOfPrimaryKey();
    bool IsPartOfOwnerKey();
}