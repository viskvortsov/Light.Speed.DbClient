namespace LightSpeed.DbClient.Database;

public interface IConnection : IDisposable, IAsyncDisposable
{

    Task<ITransaction> BeginTransactionAsync();

}