using System.Xml.XPath;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Database;

public class SortingElement<T> : ISortingElement<T> where T : IDatabaseElement
{
    
    private readonly IColumnReflection _column;
    private readonly SortingDirection _direction;
    
    public SortingElement(string name, SortingDirection direction)
    {
        DatabaseObjectReflection reflection = ClientSettings.GetReflection(typeof(T));
        _column = reflection.GetColumnReflection(name);
        _direction = direction;
    }
    
    public bool IsTableSortingElement()
    {
        throw new NotImplementedException();
    }

    public IColumnReflection Column()
    {
        throw new NotImplementedException();
    }

    public SortingDirection Direction()
    {
        throw new NotImplementedException();
    }
    
}