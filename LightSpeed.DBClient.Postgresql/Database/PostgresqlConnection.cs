using LightSpeed.DbClient.Database;
using Npgsql;

namespace LightSpeed.DbClient.Postgresql.Database;

public class PostgresqlConnection : IConnection
{
    protected internal readonly NpgsqlConnection InnerConnection;
    
    internal PostgresqlConnection(NpgsqlConnection innerConnection)
    {
        InnerConnection = innerConnection;
    }
    
    public async Task<ITransaction> BeginTransactionAsync()
    {
        NpgsqlTransaction innerTransaction = await InnerConnection.BeginTransactionAsync();
        return new PostgresqlTransaction(innerTransaction);
    }

    public void Dispose()
    {
        InnerConnection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await InnerConnection.DisposeAsync();
    }

    
}