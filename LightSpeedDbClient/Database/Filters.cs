using System.Collections;
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public class Filters<T> : IFilters<T> where T : IDatabaseElement
{
    
    private readonly IList<Filter<T>> _filters = new List<Filter<T>>();
    
    public void Add(Filter<T> filter)
    {
        _filters.Add(filter);
    }

    public IFilters<T> ConnectedTableFilters()
    {
        Filters<T> filters = new();
        foreach (var filter in _filters)
        {
            if (filter.IsTableFilter())
                filters.Add(filter);
        }
        return filters;
    }

    public IFilters<T> MainTableFilters()
    {
        Filters<T> filters = new();
        foreach (var filter in _filters)
        {
            if (!filter.IsTableFilter())
                filters.Add(filter);
        }
        return filters;
    }

    public IEnumerator<Filter<T>> GetEnumerator()
    {
        return _filters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool HasConnectedTableFilters()
    {
        return ConnectedTableFilters().Any();
    }
    
    public bool HasMainTableFilters()
    {
        return MainTableFilters().Any();
    }
    
}