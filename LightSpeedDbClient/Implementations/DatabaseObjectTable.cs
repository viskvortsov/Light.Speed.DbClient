using System.Collections;
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public class DatabaseObjectTable<E> : IDatabaseObjectTable<E> where E : IDatabaseObject
{
    
    private readonly List<E> _elements = new List<E>();

    public DatabaseObjectTable(List<IDatabaseElement> elements)
    {
        foreach (var element in elements)
        {
            _elements.Add((E) element);
        }
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

    public void Clear()
    {
        _elements.Clear();
    }

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
    public bool IsReadOnly => false;

}