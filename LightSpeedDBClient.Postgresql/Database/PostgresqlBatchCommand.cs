using LightSpeedDbClient.Database;
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
            NpgsqlDbType sqlType = PostgresqlDefaultSettings.GetSqlDbType(type, value);
            InnerCommand.Parameters.AddWithValue(name, sqlType, value);
            
        }

    }

}