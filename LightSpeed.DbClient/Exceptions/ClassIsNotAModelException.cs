namespace LightSpeed.DbClient.Exceptions;

public class ClassIsNotAModelException : Exception
{
    public ClassIsNotAModelException(string? message) : base(message)
    {
    }
    public ClassIsNotAModelException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}