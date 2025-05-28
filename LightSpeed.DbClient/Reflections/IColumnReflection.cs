using System.Reflection;
using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.Reflections;

public interface IColumnReflection
{
    string Name();
    string QueryName();
    string TranslationsQueryName();
    Type Type();
    string? Relation();
    PropertyInfo Property();
    ITableReflection Table();
    bool IsPartOfPrimaryKey();
    bool IsPartOfOwnerKey();
    bool IsTranslatable();
    bool HasForeignKeyTable();
    public string ForeignKeyName();
    ITableReflection ForeignKeyTable();
    IColumnReflection ForeignKeyColumn();
}