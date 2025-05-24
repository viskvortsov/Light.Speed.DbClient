using LightSpeedDbClient.Database;

namespace LightSpeedDBClient.Postgresql.Database;

public class PostgresqlSubQuery
{
    private readonly string _queryText;
    private readonly IQueryParameters _parameters;
    
    public PostgresqlSubQuery(string queryText, IQueryParameters parameters)
    {
        _queryText = queryText;
        _parameters = parameters;
    }

    public string QueryText => _queryText;

    public IQueryParameters Parameters => _parameters;
    
}