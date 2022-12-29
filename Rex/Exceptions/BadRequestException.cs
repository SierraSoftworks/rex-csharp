namespace Rex.Exceptions;

public class BadRequestException : Exception
{
    protected BadRequestException() : base()
    {
    }

    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException) : base(message, innerException)
    {
    }
}