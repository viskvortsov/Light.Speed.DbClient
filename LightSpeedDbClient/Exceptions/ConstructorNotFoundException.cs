namespace LightSpeedDbClient.Exceptions;

public class ConstructorNotFoundException : Exception
{
    public ConstructorNotFoundException(string? message) : base(message)
    {
    }
    public ConstructorNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}