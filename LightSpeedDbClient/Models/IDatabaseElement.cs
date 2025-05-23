namespace LightSpeedDbClient.Models;

public interface IDatabaseElement
{
    IDatabaseObjectTable Table(string name);
}