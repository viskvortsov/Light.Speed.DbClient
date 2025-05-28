namespace LightSpeed.DbClient.Models;

public interface IDatabaseObjectTableElement: IDatabaseElement
{
    IKey Key();
    IKey OwnerKey();
}