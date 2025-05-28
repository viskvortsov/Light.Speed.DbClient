using LightSpeed.DbClient.Database;

namespace LightSpeed.DbClient.Postgresql.Database;

public class PostgresqlSubQuery(string queryText, IQueryParameters parameters)
{
    public string QueryText => queryText;

    public IQueryParameters Parameters => parameters;
    
}