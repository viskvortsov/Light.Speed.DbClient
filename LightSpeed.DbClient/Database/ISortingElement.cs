using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Reflections;

namespace LightSpeed.DbClient.Database;

public interface ISortingElement<T> where T : IDatabaseElement
{
    bool IsTableSortingElement();
    IColumnReflection Column();
    SortingDirection Direction();
    string DirectionAsString();
}