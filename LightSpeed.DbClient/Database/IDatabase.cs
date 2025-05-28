namespace LightSpeed.DbClient.Database;

public interface IDatabase : IDisposable, IAsyncDisposable
{
    Task<IConnection> OpenConnectionAsync();
}