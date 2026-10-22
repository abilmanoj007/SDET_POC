namespace SDET_POC.API.Common.Exceptions;

public sealed class ConflictException : Exception
{
    public ConflictException()
        : base()
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ConflictException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was in conflict.")
    {
    }
}


