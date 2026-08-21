namespace ProjectManagement.Application.Common.Exceptions;

public sealed class UnauthorizedAccessAppException : Exception
{
    public UnauthorizedAccessAppException(string message)
        : base(message)
    {
    }
}