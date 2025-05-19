using System.Reflection;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;
using Npgsql;

namespace LightSpeedDbClient.Database;

public abstract class Manager<E> : IManager<E> where E : IDatabaseElement
{
    
    private static Dictionary<Type, ConstructorInfo> Constructors { get; } = new();
    private static Dictionary<Type, DatabaseObjectReflection> Reflections { get; } = new();
    
    protected readonly IConnection Connection;
    protected readonly ITransaction? Transaction;
    protected readonly DatabaseObjectReflection Reflection;

    public Manager(IConnection connection) : this(connection, null){}

    public Manager(IConnection connection, ITransaction? transaction)
    {
        Connection = connection;
        Transaction = transaction;
        Reflection = GetReflection(typeof(E));
    }
    
    public E Create()
    {
        return (E) GetConstructor(typeof(E)).Invoke(new object[]{});;
    }

    public abstract Task<IEnumerable<E>> GetListAsync(int? page = null, int? limit = null);
    public abstract Task<int> CountAsync();
    public abstract Task<E> GetAsync();
    public abstract Task<E> SaveAsync(E element);
    public abstract Task DeleteAsync();

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
    
    protected static ConstructorInfo GetConstructor(Type type)
    {

        lock (Constructors)
        {
            
            if (Constructors.ContainsKey(type))
            {
                return Constructors[type];
            }

            ConstructorInfo info;
            Constructors.Add(type, info = type.GetConstructor(new Type[] { }));

            return info;

        }

    }
    
    protected static DatabaseObjectReflection GetReflection(Type type)
    {
        
        if (Reflections.ContainsKey(type))
        {
            return Reflections[type];
        }

        lock (Reflections)
        {
            
            if (Reflections.ContainsKey(type))
            {
                return Reflections[type];
            }
            
            DatabaseObjectReflection reflection = new DatabaseObjectReflection(type);
            Reflections.Add(type, reflection);
            return reflection;

        }

    }
    
}