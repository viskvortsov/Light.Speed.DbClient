using LightSpeedDbClient.Database;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlSubQuery(string queryText, IQueryParameters parameters)
{
    public string QueryText => queryText;

    public IQueryParameters Parameters => parameters;
    
}