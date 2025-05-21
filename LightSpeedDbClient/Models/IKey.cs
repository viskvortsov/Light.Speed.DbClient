namespace LightSpeedDbClient.Models;

public interface IKey
{

    IEnumerable<IKeyElement> KeyElements();

}