
namespace LightSpeedDbClient.Database;

public interface IQuery
{
    string GetQueryText();
    IQueryParameters Parameters();
    
}