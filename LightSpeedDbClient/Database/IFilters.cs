using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public interface IFilters<T>: IEnumerable<IFilter<T>> where T : IDatabaseElement
{
    void Add(IFilter<T> filter);
    IFilters<T> ConnectedTableFilters();
    IFilters<T> MainTableFilters();
    bool HasConnectedTableFilters();
    bool HasMainTableFilters();
    bool HasTranslationFieldsFilters();
    bool HasNotTranslationFieldsFilters();
}