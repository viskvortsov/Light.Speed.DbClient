using LightSpeedDbClient.Database;
using LightSpeedDbClient.Models;
using Npgsql;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlDatabase(string connectionString) : IDatabase
{
    
    // https://www.npgsql.org/doc/connection-string-parameters.html
    private readonly NpgsqlDataSource _innerDatabase = NpgsqlDataSource.Create(connectionString);

    public PostgresqlDatabase(string host, int port, string database, string user, string password)
        : this($"Host={host};Port={port};Username={user};Password={password};Database={database}")
    {
    }
    
    public PostgresqlDatabase(string host, string database, string user, string password): this(host, 5432, database, user, password){}
    
    public async Task<IConnection> OpenConnectionAsync()
    {
        NpgsqlConnection innerConnection = await _innerDatabase.OpenConnectionAsync();
        PostgresqlConnection connection = new PostgresqlConnection(innerConnection);
        return connection;
    }
    
    public async ValueTask DisposeAsync()
    {
        await _innerDatabase.DisposeAsync();
    }
    
    public void Dispose()
    {
        _innerDatabase.Dispose();
    }
    
}