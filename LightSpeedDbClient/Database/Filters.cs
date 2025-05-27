using System.Collections;
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Database;

public class Filters<T> : IFilters<T> where T : IDatabaseElement
{
    
    private readonly IList<IFilter<T>> _filters = new List<IFilter<T>>();
    
    public void Add(IFilter<T> filter)
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

    public IEnumerator<IFilter<T>> GetEnumerator()
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