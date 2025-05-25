namespace LightSpeedDbClient.Exceptions;

public class ModelFieldNotFoundException : Exception
{
    public ModelFieldNotFoundException(string? message) : base(message)
    {
    }
    public ModelFieldNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}