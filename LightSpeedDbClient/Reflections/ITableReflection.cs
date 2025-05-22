namespace LightSpeedDbClient.Reflections;

public interface ITableReflection
{

    string Name();
    string QueryName();
    Type Type();
    IEnumerable<IColumnReflection> Columns();
    IEnumerable<IColumnReflection> PartsOfPrimaryKey();
    IEnumerable<IColumnReflection> PartsOfOwnerKey();
        
}