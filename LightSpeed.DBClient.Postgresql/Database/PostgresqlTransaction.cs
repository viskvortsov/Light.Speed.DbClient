using LightSpeed.DbClient.Database;
using Npgsql;

namespace LightSpeed.DbClient.Postgresql.Database;

public class PostgresqlTransaction(NpgsqlTransaction innerTransaction) : ITransaction
{
    protected internal readonly NpgsqlTransaction InnerTransaction = innerTransaction;

    public async Task CommitAsync()
    {
        await InnerTransaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        await InnerTransaction.RollbackAsync();
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