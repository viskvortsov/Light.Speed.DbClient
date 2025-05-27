using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public interface ISorting<T>: IEnumerable<ISortingElement<T>> where T : IDatabaseElement
{
    void Add(ISortingElement<T> sortingElement);
    ISortingElement<T> ConnectedTableSortingElements();
    ISortingElement<T> MainTableSortingElements();
    bool HasConnectedTableSortingElements();
    bool HasMainTableSortingElements();
}