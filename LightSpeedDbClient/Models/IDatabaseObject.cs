namespace LightSpeedDbClient.Models;

public interface IDatabaseObject: IDatabaseElement
{
    IKey Key();
}