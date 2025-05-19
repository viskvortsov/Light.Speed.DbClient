namespace LightSpeedDbClient.Database;

public interface IDatabase : IDisposable, IAsyncDisposable
{
    Task<IConnection> OpenConnectionAsync();
}