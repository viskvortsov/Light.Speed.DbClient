namespace LightSpeedDbClient.Models;

public interface IDatabaseObjectTableElement: IDatabaseElement
{
    IKey Key();
    IKey OwnerKey();
}