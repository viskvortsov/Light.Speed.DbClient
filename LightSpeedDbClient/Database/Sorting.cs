using System.Collections;
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public class Sorting<T> : ISorting<T> where T : IDatabaseElement
{
    
    public IEnumerator<ISortingElement<T>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(ISortingElement<T> sortingElement)
    {
        throw new NotImplementedException();
    }

    public ISortingElement<T> ConnectedTableSortingElements()
    {
        throw new NotImplementedException();
    }

    public ISortingElement<T> MainTableSortingElements()
    {
        throw new NotImplementedException();
    }

    public bool HasConnectedTableSortingElements()
    {
        throw new NotImplementedException();
    }

    public bool HasMainTableSortingElements()
    {
        throw new NotImplementedException();
    }
    
}