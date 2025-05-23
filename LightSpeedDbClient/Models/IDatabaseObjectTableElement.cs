namespace LightSpeedDbClient.Models;

public interface IDatabaseObjectTableElement
{
    IKey Key();
    IKey OwnerKey();
}