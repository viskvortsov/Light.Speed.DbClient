using System.Xml.XPath;
using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Reflections;

namespace LightSpeed.DbClient.Database;

public class SortingElement<T> : ISortingElement<T> where T : IDatabaseElement
{
    
    private readonly IColumnReflection _column;
    private readonly SortingDirection _direction;
    
    public SortingElement(string name, SortingDirection direction)
    {
        DatabaseObjectReflection reflection = ClientSettings.GetReflection(typeof(T));
        _column = reflection.GetColumnReflectionByQueryName(name);
        _direction = direction;
    }
    
    public bool IsTableSortingElement()
    {
        return true;
    }

    public IColumnReflection Column()
    {
        return _column;
    }

    public SortingDirection Direction()
    {
        return _direction;
    }

    public string DirectionAsString()
    {
        if (_direction == SortingDirection.Descending)
        {
            return "desc";
        }
        else
        {
            return "asc";
        }
    }
}