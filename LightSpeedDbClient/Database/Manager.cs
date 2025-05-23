using System.Reflection;
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
    
    public E Create()
    {
        return (E) ClientSettings.GetConstructor(typeof(E)).Invoke(new object[]{});;
    }
    
    public object CreateRow(Type type)
    {
        return ClientSettings.GetConstructor(type).Invoke(new object[]{});;
    }

    public abstract Task<IEnumerable<E>>
        GetListAsync(IEnumerable<IFilter> filters, int? page = null, int? limit = null);
    public abstract Task<IEnumerable<E>> GetListAsync(int? page = null, int? limit = null);

    public abstract Task<IEnumerable<E>> GetListObjectsAsync(IEnumerable<IFilter> filters, int? page = null,
        int? limit = null);

    public abstract Task<IEnumerable<E>> GetListObjectsAsync(int? page = null, int? limit = null);

    public abstract Task<int> CountAsync(IEnumerable<IFilter> filters);
    public abstract Task<int> CountAsync();
    public abstract Task<E> GetByKeyAsync(IKey key);
    public abstract Task<E> SaveAsync(E element);
    public abstract Task<IEnumerable<E>> SaveManyAsync(IEnumerable<E> elements);

    public abstract Task<int> DeleteAsync(IEnumerable<IFilter> filters);
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