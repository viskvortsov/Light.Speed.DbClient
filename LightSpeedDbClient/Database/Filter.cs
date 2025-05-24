using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Database;

public class Filter<E> : IFilter<E> where E : IDatabaseElement
{
    
    private bool _isTableFilter;
    private readonly IColumnReflection _column;
    private readonly ComparisonOperator _operator;
    private readonly object _value;
    
    public Filter(string name, ComparisonOperator comparisonOperator, object value)
    {
        DatabaseObjectReflection reflection = ClientSettings.GetReflection(typeof(E));
        
        _isTableFilter = false;
        string[] parts = name.Split('.');
        if (parts.Length > 1)
            _isTableFilter = true;

        if (_isTableFilter)
        {
            string tableName = parts[0];
            string columnName = parts[1];
            IConnectedTable connectedTable = reflection.GetTableReflection(tableName);
            if (connectedTable == null)
                throw new ReflectionException(); // TODO
            IColumnReflection column = connectedTable.TableReflection().GetColumnReflection(columnName);
            if (column == null)
                throw new ModelFieldNotFoundException();
            _column = column;
            _operator = comparisonOperator;
            _value = value;
        }
        else
        {
            IColumnReflection column = reflection.GetColumnReflection(name);
            if (column == null)
                throw new ModelFieldNotFoundException();
            _column = column;
            _operator = comparisonOperator;
            _value = value;
        }
        
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

    public bool IsTableFilter()
    {
        return _isTableFilter;
    }
    
}