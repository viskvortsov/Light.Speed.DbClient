namespace LightSpeed.DbClient.Database;

public interface IQueryParameter
{
    string Name();
    object? Value();
    Type Type();
}