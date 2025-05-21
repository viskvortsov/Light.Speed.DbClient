using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Database;

public class Filter<E> : IFilter where E : IDatabaseElement
{
    
    private readonly IColumnReflection _column;
    private readonly ComparisonOperator _operator;
    private readonly object _value;
    
    public Filter(string name, ComparisonOperator comparisonOperator, object value)
    {
        DatabaseObjectReflection reflection = ClientSettings.GetReflection(typeof(E));
        IColumnReflection column = reflection.GetColumnReflection(name);
        if (column == null)
            throw new ModelFieldNotFoundException();
        _column = column;
        _operator = comparisonOperator;
        _value = value;
    }
    
    public Filter(IColumnReflection column, ComparisonOperator comparisonOperator, object value)
    {
        _column = column;
        _operator = comparisonOperator;
        _value = value;
    }

    public IColumnReflection Column()
    {
        return _column;
    }

    public ComparisonOperator Operator()
    {
        return _operator;
    }

    public object Value()
    {
        return _value;
    }
    
}