namespace LightSpeedDbClient.Database;

public class QueryParameter : IQueryParameter
{
    
    private readonly string _name;
    private readonly Type _type;
    private readonly object _value;
    
    public QueryParameter(string name, Type type, object value)
    {
        _name = name;
        _type = type;
        _value = value;
    }

    public string Name()
    {
        return _name;
    }

    public object Value()
    {
        return _value;
    }

    public Type Type()
    {
        return _type;
    }
}