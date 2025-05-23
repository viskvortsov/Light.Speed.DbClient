namespace LightSpeedDbClient.Database;

public interface IQueryParameters : IEnumerable<IQueryParameter>
{
    string Add(Type type, object value);
    void Clear();
    int Count();
}