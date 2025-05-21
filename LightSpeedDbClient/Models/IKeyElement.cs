using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Models;

public interface IKeyElement
{
    IColumnReflection Column();
    object Value();
}