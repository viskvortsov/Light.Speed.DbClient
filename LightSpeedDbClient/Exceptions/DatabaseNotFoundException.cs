namespace LightSpeedDbClient.Exceptions;

public class DatabaseNotFoundException : Exception
{
    
    public DatabaseNotFoundException(string? message) : base(message)
    {
    }
    public DatabaseNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}