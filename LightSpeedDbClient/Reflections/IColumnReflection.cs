using System.Reflection;
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Reflections;

public interface IColumnReflection
{
    string Name();
    string QueryName();
    Type Type();
    IQueryType QueryType();
    string? Relation();
    PropertyInfo Property();
    ITableReflection Table();
    bool IsPartOfPrimaryKey();
    bool IsPartOfOwnerKey();
    bool HasAdditionalFields();
    bool HasForeignKeyTable();
    IEnumerable<IColumnReflection?> AdditionalFields();
    public string ForeignKeyName();
    ITableReflection ForeignKeyTable();
    IColumnReflection ForeignKeyColumn();
}