using Npgsql;

namespace LightSpeed.DbClient.Postgresql.Database;

public class PostgresqlBatch(PostgresqlConnection connection, PostgresqlTransaction? transaction = null)
    : IDisposable, IAsyncDisposable
{
    private readonly NpgsqlBatch _batch = new(connection.InnerConnection, transaction?.InnerTransaction);

    internal void AddCommand(PostgresqlBatchCommand command)
    {
        _batch.BatchCommands.Add(command.InnerCommand);
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