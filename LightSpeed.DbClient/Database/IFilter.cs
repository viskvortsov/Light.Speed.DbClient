using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Reflections;

namespace LightSpeed.DbClient.Database;

public interface IFilter<T> where T : IDatabaseElement
{
    bool IsTableFilter();
    bool IsTranslationFieldsFilter();
    IColumnReflection Column();
    ComparisonOperator Operator();
    object? Value();
    Type Type();
}