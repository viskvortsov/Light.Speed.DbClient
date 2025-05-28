using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public interface IManager<T> : IDisposable, IAsyncDisposable where T : IDatabaseElement
{
    
    // TODO Change all owner keys with primary keys before save
    // TODO go through all todos
    // TODO exceptions
    // TODO debug logging and requests logging
    // TODO static analysis and optimization
    
    // TODO add RecordSets
    
    // TODO sorting
    // translations filtration
    // pagination test

    T CreateObject();
    T CreateReference();
    IFilters<T> CreateFilters();
    Task<IEnumerable<T>> GetListAsync(IFilters<T> filters, int? page = null, int? limit = null);
    Task<IEnumerable<T>> GetListAsync(IFilters<T> filters, ISorting<T> sortBy, int? page = null, int? limit = null);
    Task<IEnumerable<T>> GetListAsync(ISorting<T> sortBy, int? page = null, int? limit = null);
    Task<IEnumerable<T>> GetListAsync(int? page = null, int? limit = null);
    
    Task<IEnumerable<T>> GetListObjectsAsync(IFilters<T> filters, int? page = null, int? limit = null);
    Task<IEnumerable<T>> GetListObjectsAsync(IFilters<T> filters, ISorting<T> sortBy, int? page = null, int? limit = null);
    Task<IEnumerable<T>> GetListObjectsAsync(ISorting<T> sortBy, int? page = null, int? limit = null);
    Task<IEnumerable<T>> GetListObjectsAsync(int? page = null, int? limit = null);
    Task<T> GetByKeyAsync(IKey key);
    Task<int> CountAsync(IFilters<T> filters);
    Task<int> CountAsync();
    Task<T> SaveAsync(T element);
    Task<IEnumerable<T>> SaveManyAsync(IEnumerable<T> elements, int chunkSize = 1000);
    Task<int> DeleteAsync();
    Task<int> DeleteAsync(IFilters<T> filters);
    Task<int> DeleteByKeyAsync(IKey key);
    
}