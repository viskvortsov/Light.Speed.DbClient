using System.Data;
using LightSpeedDbClient.Database;
using Npgsql;

namespace LightSpeedDBClient.Postgresql.Database;

internal class PostgresqlCommand: IDisposable, IAsyncDisposable
{
    
    private readonly NpgsqlCommand _innerCommand;

    internal PostgresqlCommand(IQuery query, PostgresqlConnection connection, PostgresqlTransaction? transaction = null)
    {
        _innerCommand = new NpgsqlCommand(query.GetQueryText(), connection.InnerConnection, transaction?.InnerTransaction);
    }

    internal async Task<NpgsqlDataReader> ExecuteReaderAsync()
    {
        return await _innerCommand.ExecuteReaderAsync();
    }

    public void Dispose()
    {
        _innerCommand.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _innerCommand.DisposeAsync();
    }
    
}