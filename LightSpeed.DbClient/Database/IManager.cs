using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.Database;

public interface IManager<T> : IDisposable, IAsyncDisposable where T : IDatabaseElement
{
    
    // TODO Change all owner keys with primary keys before save
    // TODO go through all todos
    // TODO exceptions
    // TODO debug logging and requests logging
    // TODO static analysis and optimization
    
    // TODO add RecordSets
    
    // pagination test
    
    // TODO sorting by translated field
    
    T CreateObject();
    T CreateReference();
    IFilters<T> CreateFilters();
    Task<IDataSelection<T>> GetListAsync(IFilters<T> filters, int? page = null, int? limit = null);
    Task<IDataSelection<T>> GetListAsync(IFilters<T> filters, ISorting<T> sortBy, int? page = null, int? limit = null);
    Task<IDataSelection<T>> GetListAsync(ISorting<T> sortBy, int? page = null, int? limit = null);
    Task<IDataSelection<T>> GetListAsync(int? page = null, int? limit = null);
    
    Task<IDataSelection<T>> GetListObjectsAsync(IFilters<T> filters, int? page = null, int? limit = null);
    Task<IDataSelection<T>> GetListObjectsAsync(IFilters<T> filters, ISorting<T> sortBy, int? page = null, int? limit = null);
    Task<IDataSelection<T>> GetListObjectsAsync(ISorting<T> sortBy, int? page = null, int? limit = null);
    Task<IDataSelection<T>> GetListObjectsAsync(int? page = null, int? limit = null);
    Task<T> GetByKeyAsync(IKey key);
    Task<long> CountAsync(IFilters<T> filters);
    Task<long> CountAsync();
    Task<T> SaveAsync(T element);
    Task<IDataSelection<T>> SaveManyAsync(IEnumerable<T> elements, int chunkSize = 1000);
    Task<int> DeleteAsync();
    Task<int> DeleteAsync(IFilters<T> filters);
    Task<int> DeleteByKeyAsync(IKey key);
    
}