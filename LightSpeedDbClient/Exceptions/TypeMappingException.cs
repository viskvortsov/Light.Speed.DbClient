namespace LightSpeedDbClient.Exceptions;

public class TypeMappingException : Exception
{
    public TypeMappingException(string? message) : base(message)
    {
    }
    public TypeMappingException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}