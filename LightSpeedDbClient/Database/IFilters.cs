using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public interface IFilters<E>: IEnumerable<Filter<E>> where E : IDatabaseElement
{
    void Add(Filter<E> filter);
    IFilters<E> ConnectedTableFilters();
    IFilters<E> MainTableFilters();
}