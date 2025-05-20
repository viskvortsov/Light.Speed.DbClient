namespace LightSpeedDbClient.Exceptions;

public class DatabaseSaveException : Exception
{
    
    public DatabaseSaveException(string? message) : base(message)
    {
    }
    public DatabaseSaveException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}