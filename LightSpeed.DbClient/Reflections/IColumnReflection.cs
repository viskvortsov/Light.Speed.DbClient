using System.Reflection;
using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.Reflections;

public interface IColumnReflection
{
    bool IsReadOnly();
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
    bool HasAdditionalForeignKeyTable();
    public string ForeignKeyName();
    public string AdditionalForeignKeyName();
    ITableReflection ForeignKeyTable();
    ITableReflection AdditionalForeignKeyTable();
    IForeignKeyTable ForeignKeyTableLink();
    IColumnReflection ForeignKeyColumn();
    IColumnReflection AdditionalForeignKeyColumn();
    string AdditionalForeignKeyColumnName();
}