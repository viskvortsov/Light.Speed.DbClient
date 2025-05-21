using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public interface IManager<E> : IDisposable, IAsyncDisposable where E : IDatabaseElement
{
    
    E Create();
    Task<IEnumerable<E>> GetListAsync(int? page = null, int? limit = null);
    Task<E> GetByKeyAsync(IKey key);
    Task<int> CountAsync();
    Task<E> SaveAsync(E element);
    Task DeleteAsync();
    
}