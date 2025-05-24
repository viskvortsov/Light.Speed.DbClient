using System.Collections;
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public class Filters<E> : IFilters<E> where E : IDatabaseElement
{
    
    private readonly IList<Filter<E>> _filters = new List<Filter<E>>();
    
    public void Add(Filter<E> filter)
    {
        _filters.Add(filter);
    }

    public IEnumerator<Filter<E>> GetEnumerator()
    {
        return _filters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
}