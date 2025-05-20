namespace LightSpeedDbClient.Database;

public interface IQueryParameters : IEnumerable<IQueryParameter>
{
    void Add(IQueryParameter parameter);
}