namespace LightSpeed.DbClient.Exceptions;

public class ReflectionException : Exception
{
    public ReflectionException(string? message) : base(message)
    {
    }
    public ReflectionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}