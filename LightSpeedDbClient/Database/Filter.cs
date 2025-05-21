using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Database;

public class Filter : IFilter
{
    
    private readonly IColumnReflection _column;
    private readonly ComparisonOperator _operator;
    private readonly object _value;
    
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