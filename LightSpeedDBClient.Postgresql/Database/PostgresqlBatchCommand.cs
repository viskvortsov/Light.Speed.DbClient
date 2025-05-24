using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using Npgsql;
using NpgsqlTypes;

namespace LightSpeedDBClient.Postgresql.Database;

internal class PostgresqlBatchCommand
{
    
    internal readonly NpgsqlBatchCommand _innerCommand;

    internal PostgresqlBatchCommand(PostgresqlSubQuery query)
    {

        _innerCommand = new NpgsqlBatchCommand(
            query.QueryText
        );
        
        foreach (IQueryParameter parameter in query.Parameters)
        {
            
            string name = parameter.Name();
            
            object? value = parameter.Value();
            if (value == null)
            {
                value = DBNull.Value;
            }
            
            Type type = parameter.Type();
            PostgresqlDefaultSettings.DefaultTypes.TryGetValue(type, out NpgsqlDbType sqlType);
            if (sqlType == null)
            {
                throw new TypeMappingException();
            }
            
            _innerCommand.Parameters.AddWithValue(name, sqlType, value);
            
        }

    }

}