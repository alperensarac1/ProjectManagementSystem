namespace ProjectManagement.Application.Common.Exceptions;

public sealed class RequestValidationException : Exception
{

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public RequestValidationException(
        IReadOnlyDictionary<string, string[]> errors)
        : base("Gönderilen bilgiler geçerli değildir.")
    {
        Errors = errors;
    }

    public RequestValidationException(
        string message,
        IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }
}