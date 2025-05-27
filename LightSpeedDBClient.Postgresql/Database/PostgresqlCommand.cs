using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using Npgsql;
using NpgsqlTypes;

namespace LightSpeedDBClient.Postgresql.Database;

internal class PostgresqlCommand: IDisposable, IAsyncDisposable
{
    
    private readonly NpgsqlCommand _innerCommand;

    internal PostgresqlCommand(IQuery query, PostgresqlConnection connection, PostgresqlTransaction? transaction = null)
    {

        _innerCommand = new NpgsqlCommand(
            query.GetQueryText(),
            connection.InnerConnection,
            transaction?.InnerTransaction
        );
        
        foreach (IQueryParameter parameter in query.Parameters())
        {
            
            string name = parameter.Name();
            Type type = parameter.Type();
            object? value = parameter.Value();
            NpgsqlDbType sqlType = PostgresqlDefaultSettings.GetSqlDbType(type, value);
            value = PostgresqlDefaultSettings.ConvertValue(value);
            _innerCommand.Parameters.AddWithValue(name, sqlType, value);
            
        }

    }

    internal async Task<NpgsqlDataReader> ExecuteReaderAsync()
    {
        return await _innerCommand.ExecuteReaderAsync();
    }
    
    internal async Task<int> ExecuteNonQueryAsync()
    {
        return await _innerCommand.ExecuteNonQueryAsync();
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