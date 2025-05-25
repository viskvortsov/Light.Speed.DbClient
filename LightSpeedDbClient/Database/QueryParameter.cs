namespace LightSpeedDbClient.Database;

public class QueryParameter(string name, Type type, object? value) : IQueryParameter
{
    public string Name()
    {
        return name;
    }

    public object? Value()
    {
        return value;
    }

    public Type Type()
    {
        return type;
    }
}