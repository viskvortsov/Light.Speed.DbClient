using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Database;

public interface IFilter<T> where T : IDatabaseElement
{
    bool IsTableFilter();
    IColumnReflection Column();
    ComparisonOperator Operator();
    object? Value();
    Type Type();
}