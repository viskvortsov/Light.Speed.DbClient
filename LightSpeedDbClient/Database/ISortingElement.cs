using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Database;

public interface ISortingElement<T> where T : IDatabaseElement
{
    bool IsTableSortingElement();
    IColumnReflection Column();
    SortingDirection Direction();
    string DirectionAsString();
}