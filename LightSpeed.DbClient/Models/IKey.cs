namespace LightSpeed.DbClient.Models;

public interface IKey
{
    IEnumerable<IKeyElement> KeyElements();
}