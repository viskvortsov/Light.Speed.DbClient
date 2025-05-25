using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public interface IFilters<T>: IEnumerable<Filter<T>> where T : IDatabaseElement
{
    void Add(Filter<T> filter);
    IFilters<T> ConnectedTableFilters();
    IFilters<T> MainTableFilters();
    bool HasConnectedTableFilters();
    bool HasMainTableFilters();
}