using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using Npgsql;
using NpgsqlTypes;

namespace LightSpeedDBClient.Postgresql.Database;

internal class PostgresqlBatchCommand
{
    
    internal readonly NpgsqlBatchCommand InnerCommand;

    internal PostgresqlBatchCommand(PostgresqlSubQuery query)
    {

        InnerCommand = new NpgsqlBatchCommand(
            query.QueryText
        );
        
        foreach (IQueryParameter parameter in query.Parameters)
        {
            
            string name = parameter.Name();
            
            object value = parameter.Value() ?? DBNull.Value;

            Type type = parameter.Type();
            PostgresqlDefaultSettings.DefaultTypes.TryGetValue(type, out NpgsqlDbType sqlType);
            if (sqlType == null)
            {
                throw new TypeMappingException($"Incompatible type {type} for parameter {name}.");
            }
            
            InnerCommand.Parameters.AddWithValue(name, sqlType, value);
            
        }

    }

}