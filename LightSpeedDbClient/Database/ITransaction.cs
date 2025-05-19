namespace LightSpeedDbClient.Database;

public interface ITransaction : IDisposable, IAsyncDisposable
{

    Task CommitAsync();
    Task RollbackAsync();

}