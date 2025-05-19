using LightSpeedDbClient.Database;
using Npgsql;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlTransaction : ITransaction
{
    protected internal NpgsqlTransaction InnerTransaction;

    public PostgresqlTransaction(NpgsqlTransaction innerTransaction)
    {
        InnerTransaction = innerTransaction;
    }
    
    public async Task CommitAsync()
    {
        await InnerTransaction.CommitAsync();
    }

    public Task RollbackAsync()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        InnerTransaction.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await InnerTransaction.DisposeAsync();
    }
    
}