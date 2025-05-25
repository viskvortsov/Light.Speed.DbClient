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
            object value = parameter.Value() ?? DBNull.Value;
 
            Type type = parameter.Type();
            PostgresqlDefaultSettings.DefaultTypes.TryGetValue(type, out NpgsqlDbType sqlType);
            if (sqlType == null)
            {
                throw new TypeMappingException($"Incompatible type {type} for parameter {name}.");
            }
            
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