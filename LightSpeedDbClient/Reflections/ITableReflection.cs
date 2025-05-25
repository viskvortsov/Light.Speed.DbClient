namespace LightSpeedDbClient.Reflections;

public interface ITableReflection
{

    string Name();
    string QueryName();
    Type Type();
    IEnumerable<IColumnReflection> Columns();
    IEnumerable<IColumnReflection> PartsOfPrimaryKey();
    IEnumerable<IColumnReflection> PartsOfOwnerKey();
    IColumnReflection? GetColumnReflection(string name);
    IColumnReflection GetTableReflection(string name);
    IEnumerable<IColumnReflection> ColumnsWithForeignKey();
    IEnumerable<IColumnReflection> ColumnsWithAdditionalInfo(string foreignKeyName);
    IEnumerable<IColumnReflection> AdditionalFields();
    IColumnReflection GetForeignKeyColumn(string name);

}