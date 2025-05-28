using LightSpeed.DbClient.Reflections;

namespace LightSpeed.DbClient.Models;

public interface IKeyElement
{
    IColumnReflection Column();
    object? Value();
    Type Type();
}