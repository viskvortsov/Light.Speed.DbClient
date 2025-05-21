using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public interface IManager<E> : IDisposable, IAsyncDisposable where E : IDatabaseElement
{
    
    E Create();
    Task<IEnumerable<E>> GetListAsync(IEnumerable<IFilter> filters, int? page = null, int? limit = null);
    Task<IEnumerable<E>> GetListAsync(int? page = null, int? limit = null);
    Task<E> GetByKeyAsync(IKey key);
    Task<int> CountAsync(IEnumerable<IFilter> filters);
    Task<int> CountAsync();
    Task<E> SaveAsync(E element);
    Task DeleteAsync(IEnumerable<IFilter> filters);
    Task DeleteByKeyAsync(IKey key);
    
}