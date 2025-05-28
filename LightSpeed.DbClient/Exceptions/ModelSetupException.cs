namespace LightSpeed.DbClient.Exceptions;

public class ModelSetupException : Exception
{
    public ModelSetupException(string? message) : base(message)
    {
    }
    public ModelSetupException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}