using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public interface IManager<E> : IDisposable, IAsyncDisposable where E : IDatabaseElement
{
    
    // TODO add connected tables filter to GetListAsync, CountAsync, DeleteAsync
    // TODO add GetListObjectsAsync
    // TODO add SaveManyAsync
    // TODO add difference with Reference and Object
    
    // TODO add Additional fields
    // TODO add localization
    
    E Create();
    Task<IEnumerable<E>> GetListAsync(IEnumerable<IFilter> filters, int? page = null, int? limit = null);
    Task<IEnumerable<E>> GetListAsync(int? page = null, int? limit = null);
    Task<E> GetByKeyAsync(IKey key);
    Task<int> CountAsync(IEnumerable<IFilter> filters);
    Task<int> CountAsync();
    Task<E> SaveAsync(E element);
    Task<IEnumerable<E>> SaveManyAsync(IEnumerable<E> elements);
    Task<int> DeleteAsync();
    Task<int> DeleteAsync(IEnumerable<IFilter> filters);
    Task<int> DeleteByKeyAsync(IKey key);
    
}