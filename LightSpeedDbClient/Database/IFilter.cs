using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Database;

public interface IFilter<E> where E : IDatabaseElement
{
    IColumnReflection Column();
    ComparisonOperator Operator();
    object Value();
}