using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public interface IManager<E> : IDisposable, IAsyncDisposable where E : IDatabaseElement
{
    
    // TODO add connected tables filter to GetListAsync, CountAsync, DeleteAsync
    
    // TODO add localization
    
    // TODO Change all owner keys with primary keys before save

    E CreateObject();
    E CreateReference();
    IFilters<E> CreateFilters();
    Task<IEnumerable<E>> GetListAsync(IFilters<E> filters, int? page = null, int? limit = null);
    Task<IEnumerable<E>> GetListAsync(int? page = null, int? limit = null);
    Task<IEnumerable<E>> GetListObjectsAsync(IFilters<E> filters, int? page = null, int? limit = null);
    Task<IEnumerable<E>> GetListObjectsAsync(int? page = null, int? limit = null);
    Task<E> GetByKeyAsync(IKey key);
    Task<int> CountAsync(IFilters<E> filters);
    Task<int> CountAsync();
    Task<E> SaveAsync(E element);
    Task<IEnumerable<E>> SaveManyAsync(IEnumerable<E> elements, int chunkSize = 1000);
    Task<int> DeleteAsync();
    Task<int> DeleteAsync(IFilters<E> filters);
    Task<int> DeleteByKeyAsync(IKey key);
    
}