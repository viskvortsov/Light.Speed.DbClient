namespace LightSpeedDbClient.Reflections;

public interface ITableReflection
{
    
    string QueryName();
    IEnumerable<IColumnReflection> Columns();
}