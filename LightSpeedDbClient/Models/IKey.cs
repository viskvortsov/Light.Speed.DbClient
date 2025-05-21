namespace LightSpeedDbClient.Models;

public interface IKey
{

    IEnumerable<IKeyElement> KeyElements();
    object GetValue(string name);

}