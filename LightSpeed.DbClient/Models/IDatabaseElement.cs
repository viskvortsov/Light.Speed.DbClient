namespace LightSpeed.DbClient.Models;

public interface IDatabaseElement
{
    ModelType ModelType();
    bool IsObject();
    bool IsReference();
    bool IsRow();
}