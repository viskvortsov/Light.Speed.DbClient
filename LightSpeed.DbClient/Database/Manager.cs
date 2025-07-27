using System.Reflection;
using LightSpeed.DbClient.Exceptions;
using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Reflections;

namespace LightSpeed.DbClient.Database;

public abstract class Manager<T>(IConnection connection, ITransaction? transaction = null) : IManager<T>
    where T : IDatabaseElement
{
    protected readonly IConnection Connection = connection;
    protected readonly ITransaction? Transaction = transaction;
    protected static readonly DatabaseObjectReflection Reflection = ClientSettings.GetReflection(typeof(T));
    
    public T CreateObject()
    {
        ConstructorInfo? constructor = ClientSettings.GetConstructor(typeof(T));
        if (constructor == null)
            throw new ConstructorNotFoundException($"Constructor not found fot type {typeof(T).Name}");
        return (T) constructor.Invoke([ModelType.Object]);
    }
    
    public T CreateReference()
    {
        ConstructorInfo? constructor = ClientSettings.GetConstructor(typeof(T));
        if (constructor == null)
            throw new ConstructorNotFoundException($"Constructor not found fot type {typeof(T).Name}");
        return (T) constructor.Invoke([ModelType.Reference]);
    }
    
    public IFilters<T> CreateFilters()
    {
        return new Filters<T>();
    }

    

    protected object CreateRow(Type type)
    {

        ConstructorInfo? constructor = ClientSettings.GetConstructor(type);
        if (constructor == null)
            throw new ConstructorNotFoundException($"Constructor not found fot type {type.Name}");
        return constructor.Invoke([ModelType.Row]);
    }
    
    public abstract Task<IDataSelection<T>> GetListAsync(IFilters<T> filters, int? page = null, int? limit = null);
    public abstract Task<IDataSelection<T>> GetListAsync(ISorting<T> sortBy, int? page = null, int? limit = null);
    public abstract Task<IDataSelection<T>> GetListAsync(int? page = null, int? limit = null);
    
    public abstract Task<IDataSelection<T>> GetListAsync(IFilters<T> filters, ISorting<T> sortBy, int? page = null,
        int? limit = null);
    
    public abstract Task<IDataSelection<T>> GetListObjectsAsync(IFilters<T> filters, int? page = null, int? limit = null);
    public abstract Task<IDataSelection<T>> GetListObjectsAsync(IFilters<T> filters, ISorting<T> sortBy,
        int? page = null, int? limit = null);
    public abstract Task<IDataSelection<T>> GetListObjectsAsync(ISorting<T> sortBy, int? page = null,
        int? limit = null);
    public abstract Task<IDataSelection<T>> GetListObjectsAsync(int? page = null, int? limit = null);

    public abstract Task<long> CountAsync(IFilters<T> filters);
    public abstract Task<long> CountAsync();
    public abstract Task<T> GetByKeyAsync(IKey key);
    public abstract Task<T> SaveAsync(T element);
    public abstract Task<IDataSelection<T>> SaveManyAsync(IEnumerable<T> elements, int chunkSize = 1000);
    public abstract Task<T> SaveRecordsAsync(IFilters<T> filters, T element);
    public abstract Task<IDataSelection<T>> SaveRecordsAsync(IFilters<T> filters, IEnumerable<T> elements, int chunkSize = 1000);
    public abstract Task<int> DeleteAsync(IFilters<T> filters);
    public abstract Task<int> DeleteByKeyAsync(IKey key);
    public abstract Task<int> DeleteAsync();

    public void Dispose()
    {
        Connection.Dispose();
        Transaction?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
        if (Transaction != null) await Transaction.DisposeAsync();
    }
    
}