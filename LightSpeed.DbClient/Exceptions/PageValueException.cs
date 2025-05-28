namespace LightSpeed.DbClient.Exceptions;

public class PageValueException : Exception
{
    public PageValueException(string? message) : base(message)
    {
    }
    public PageValueException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}