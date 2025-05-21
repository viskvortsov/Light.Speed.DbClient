namespace LightSpeedDbClient.Reflections;

public interface ITableReflection
{

    string Name();
    string QueryName();
    IEnumerable<IColumnReflection> Columns();
    IEnumerable<IColumnReflection> PartsOfPrimaryKey();
}