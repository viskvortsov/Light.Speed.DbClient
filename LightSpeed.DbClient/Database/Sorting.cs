using System.Collections;
using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.Database;

public class Sorting<T> : ISorting<T> where T : IDatabaseElement // TODO make it collection itself
{
    
    private readonly IList<ISortingElement<T>> _sortingElements = new List<ISortingElement<T>>();
    
    public IEnumerator<ISortingElement<T>> GetEnumerator()
    {
        return _sortingElements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(ISortingElement<T> sortingElement)
    {
        _sortingElements.Add(sortingElement);
    }

    public IList<ISortingElement<T>> MainTableSortingElements()
    {
        return _sortingElements;
    }

    public bool HasMainTableSortingElements()
    {
        return _sortingElements.Any();
    }
    
}