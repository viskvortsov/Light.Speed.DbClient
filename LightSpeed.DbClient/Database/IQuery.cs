
namespace LightSpeed.DbClient.Database;

public interface IQuery
{
    string GetQueryText();
    IQueryParameters Parameters();
    
}