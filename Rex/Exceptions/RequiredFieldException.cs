namespace Rex.Exceptions;

public class RequiredFieldException : BadRequestException
{
    public RequiredFieldException(string entityType, string fieldName)
        : base($"The {entityType}.{fieldName} field is required to be present and non-null.")
    {
    }

    protected RequiredFieldException() : base()
    {
    }

    private RequiredFieldException(string message) : base(message)
    {
    }

    private RequiredFieldException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
}