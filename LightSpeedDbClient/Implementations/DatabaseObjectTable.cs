using System.Collections;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public class DatabaseObjectTable<E> : IDatabaseObjectTable where E : IDatabaseObjectTableElement
{
    
    private readonly List<E> _elements = new List<E>();

    public DatabaseObjectTable(List<IDatabaseObjectTableElement> elements)
    {
        foreach (var element in elements)
        {
            _elements.Add((E) element);
        }
    }
    
    public DatabaseObjectTable() : this(ModelType.Row)
    {
    }
    
    public DatabaseObjectTable(ModelType modelType)
    {
    }

    public IEnumerator<E> GetEnumerator()
    {
        return _elements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(E item)
    {
        _elements.Add(item);
    }
    
    public int Add(object? value)
    {
        if (value is E row)
        {
            Add(row);
        }
        else
        {
            throw new ReflectionException();
        }
        return _elements.Count;
    }

    public void Clear()
    {
        _elements.Clear();
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

    public bool Contains(E item)
    {
        return _elements.Contains(item);
    }

    public void CopyTo(E[] array, int arrayIndex)
    {
        _elements.CopyTo(array, arrayIndex);
    }

    public bool Remove(E item)
    {
        return _elements.Remove(item);
    }

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