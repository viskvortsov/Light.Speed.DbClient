using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Database;

public interface IFilter
{
    IColumnReflection Column();
    ComparisonOperator Operator();
    object Value();
}