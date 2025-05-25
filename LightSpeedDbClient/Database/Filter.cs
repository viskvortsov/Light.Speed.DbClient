using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Database;

public class Filter<T> : IFilter<T> where T : IDatabaseElement
{
    
    private readonly bool _isTableFilter;
    private readonly IColumnReflection _column;
    private readonly ComparisonOperator _operator;
    private readonly Type _type;
    private readonly object? _value;
    
    public Filter(string name, ComparisonOperator comparisonOperator, object? value)
    {
        DatabaseObjectReflection reflection = ClientSettings.GetReflection(typeof(T));
        
        _isTableFilter = false;
        string?[] parts = name.Split('.');
        if (parts.Length > 1)
            _isTableFilter = true;

        if (_isTableFilter)
        {
            string tableName = parts[0]!;
            string columnName = parts[1]!;
            IConnectedTable connectedTable = reflection.GetTableReflection(tableName);
            if (connectedTable == null)
                throw new ReflectionException($"Connected table {tableName} not found");
            IColumnReflection? column = connectedTable.TableReflection().GetColumnReflection(columnName);
            _column = column ?? throw new ReflectionException($"Column {columnName} not found in table {tableName}");
            _type = column.Type();
            _operator = comparisonOperator;
            _value = value;
        }
        else
        {
            _column = reflection.GetColumnReflection(name);
            _type = _column.Type();
            _operator = comparisonOperator;
            _value = value;
        }
        
    }
    
    public Filter(IColumnReflection column, ComparisonOperator comparisonOperator, object value)
    {
        _column = column;
        _type = column.Type();
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

    public object? Value()
    {
        return _value;
    }

    public Type Type()
    {
        return _type;
    }

    public bool IsTableFilter()
    {
        return _isTableFilter;
    }
    
}