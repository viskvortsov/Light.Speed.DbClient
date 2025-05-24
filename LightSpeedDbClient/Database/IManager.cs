using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public interface IManager<E> : IDisposable, IAsyncDisposable where E : IDatabaseElement
{
    
    // TODO add connected tables filter to GetListAsync, CountAsync, DeleteAsync

    // TODO add Additional fields
    // TODO add localization
    
    // TODO Change all owner keys with primary keys before save
    
    // TODO IN filter to ListAsync and ListObjectsAsync
    
    // TODO 100 000 elements test
    
    E CreateObject();
    E CreateReference();
    Task<IEnumerable<E>> GetListAsync(IEnumerable<IFilter> filters, int? page = null, int? limit = null);
    Task<IEnumerable<E>> GetListAsync(int? page = null, int? limit = null);
    Task<IEnumerable<E>> GetListObjectsAsync(IEnumerable<IFilter> filters, int? page = null, int? limit = null);
    Task<IEnumerable<E>> GetListObjectsAsync(int? page = null, int? limit = null);
    Task<E> GetByKeyAsync(IKey key);
    Task<int> CountAsync(IEnumerable<IFilter> filters);
    Task<int> CountAsync();
    Task<E> SaveAsync(E element);
    Task<IEnumerable<E>> SaveManyAsync(IEnumerable<E> elements, int chunkSize = 1000);
    Task<int> DeleteAsync();
    Task<int> DeleteAsync(IEnumerable<IFilter> filters);
    Task<int> DeleteByKeyAsync(IKey key);
    
}