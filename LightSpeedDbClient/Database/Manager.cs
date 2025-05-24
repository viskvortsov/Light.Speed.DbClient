using System.Reflection;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Database;

public abstract class Manager<E> : IManager<E> where E : IDatabaseElement
{
    
    protected readonly IConnection Connection;
    protected readonly ITransaction? Transaction;
    protected readonly DatabaseObjectReflection Reflection;

    public Manager(IConnection connection) : this(connection, null){}

    public Manager(IConnection connection, ITransaction? transaction)
    {
        Connection = connection;
        Transaction = transaction;
        Reflection = ClientSettings.GetReflection(typeof(E));
    }
    
    public E CreateObject()
    {
        return (E) ClientSettings.GetConstructor(typeof(E)).Invoke(new object[]{ModelType.Object});;
    }
    
    public E CreateReference()
    {
        return (E) ClientSettings.GetConstructor(typeof(E)).Invoke(new object[]{ModelType.Reference});;
    }
    
    public IFilters<E> CreateFilters()
    {
        return new Filters<E>();
    }
    
    public object CreateRow(Type type)
    {

        ConstructorInfo constructor = ClientSettings.GetConstructor(type);
        if (constructor == null)
            throw new ConstructorNotFoundException(); // TODO message
        return constructor.Invoke(new object[]{ModelType.Row});;
    }

    public abstract Task<IEnumerable<E>>
        GetListAsync(IFilters<E> filters, int? page = null, int? limit = null);
    public abstract Task<IEnumerable<E>> GetListAsync(int? page = null, int? limit = null);

    public abstract Task<IEnumerable<E>> GetListObjectsAsync(IFilters<E> filters, int? page = null,
        int? limit = null);

    public abstract Task<IEnumerable<E>> GetListObjectsAsync(int? page = null, int? limit = null);

    public abstract Task<int> CountAsync(IFilters<E> filters);
    public abstract Task<int> CountAsync();
    public abstract Task<E> GetByKeyAsync(IKey key);
    public abstract Task<E> SaveAsync(E element);
    public abstract Task<IEnumerable<E>> SaveManyAsync(IEnumerable<E> elements, int chunkSize = 1000);

    public abstract Task<int> DeleteAsync(IFilters<E> filters);
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