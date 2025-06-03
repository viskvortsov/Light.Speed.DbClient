using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.Reflections;

public interface ITableReflection
{

    string Name();
    string QueryName();
    string TranslationsTableName();
    Type Type();
    bool IsTranslatable();
    IEnumerable<IColumnReflection> Columns();
    string? TranslationsTableQueryName();
    IEnumerable<IColumnReflection> TranslatableColumns();
    IEnumerable<IColumnReflection> AdditionalTranslatableColumns();
    IEnumerable<IColumnReflection> PartsOfPrimaryKey();
    IEnumerable<IColumnReflection> PartsOfOwnerKey();
    IColumnReflection? GetColumnReflection(string name);
    IColumnReflection? GetAdditionalColumnReflection(string name);
    IColumnReflection GetTableReflection(string name);
    IEnumerable<IColumnReflection> ColumnsWithForeignKey();
    IEnumerable<IColumnReflection> AdditionalFieldsWithForeignKey();
    IEnumerable<IColumnReflection> ColumnsWithAdditionalInfo(string foreignKeyName);
    IEnumerable<IColumnReflection> AdditionalFields();
    IEnumerable<IColumnReflection> AdditionalFields2();
    IColumnReflection GetForeignKeyColumn(string name);
    IColumnReflection GetAdditionalForeignKeyColumn(string name);

}