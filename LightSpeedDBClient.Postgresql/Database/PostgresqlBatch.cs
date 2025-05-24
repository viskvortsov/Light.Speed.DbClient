using Npgsql;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlBatch: IDisposable, IAsyncDisposable
{
    private NpgsqlBatch _batch;
    
    public PostgresqlBatch(PostgresqlConnection connection, PostgresqlTransaction? transaction = null)
    {
        _batch = new NpgsqlBatch(connection.InnerConnection, transaction?.InnerTransaction);
        
    }

    internal void AddCommand(PostgresqlBatchCommand command)
    {
        _batch.BatchCommands.Add(command._innerCommand);
    }

    internal async Task<int> ExecuteNonQueryAsync()
    {
        return await _batch.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        _batch.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _batch.DisposeAsync();
    }
    
}