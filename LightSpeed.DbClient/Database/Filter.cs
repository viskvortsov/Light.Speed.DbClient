using LightSpeed.DbClient.Exceptions;
using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Reflections;

namespace LightSpeed.DbClient.Database;

public class Filter<T> : IFilter<T> where T : IDatabaseElement
{
    
    private readonly bool _isTableFilter;
    private readonly bool _isTranslationFieldsFilter;
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
            _column = reflection.GetColumnReflectionByQueryName(name);
            _type = _column.Type();
            _operator = comparisonOperator;
            _value = value;
        }

        _isTranslationFieldsFilter = _column.IsTranslatable();

    }
    
    public Filter(IColumnReflection column, ComparisonOperator comparisonOperator, object value)
    {
        _column = column;
        _type = column.Type();
        _operator = comparisonOperator;
        _value = value;
    }

    public bool IsTranslationFieldsFilter()
    {
        return _isTranslationFieldsFilter;
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