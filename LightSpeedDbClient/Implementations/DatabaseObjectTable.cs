using System.Collections;
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public class DatabaseObjectTable<T> : IDatabaseObjectTable where T : IDatabaseObjectTableElement
{
    
    private readonly List<T> _elements = new List<T>();

    public DatabaseObjectTable(List<IDatabaseObjectTableElement> elements)
    {
        foreach (var element in elements)
        {
            _elements.Add((T) element);
        }
    }
    
    public DatabaseObjectTable() : this(ModelType.Row)
    {
    }
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public DatabaseObjectTable(ModelType modelType)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _elements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        _elements.Add(item);
    }

    public int Add(object? value)
    {
        if (value is T)
        {
            Add((T) value);
        }
        else
        {
            throw new ArgumentException("Value is not of type T");
        }
        return _elements.Count;
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(object? value)
    {
        throw new NotImplementedException();
    }

    public int IndexOf(object? value)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, object? value)
    {
        throw new NotImplementedException();
    }

    public void Remove(object? value)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    public bool IsFixedSize { get; }

    public void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }

    public int Count => _elements.Count;
    public bool IsSynchronized { get; }
    public object SyncRoot { get; }
    public bool IsReadOnly => false;

    public object? this[int index]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}