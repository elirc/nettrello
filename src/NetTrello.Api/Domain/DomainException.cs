namespace NetTrello.Api.Domain;

public sealed class DomainException : Exception
{
    public DomainException(string code, string message, int statusCode)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public string Code { get; }

    public int StatusCode { get; }

    public static DomainException BadRequest(string code, string message) => new(code, message, 400);

    public static DomainException NotFound(string code, string message) => new(code, message, 404);

    public static DomainException Conflict(string code, string message) => new(code, message, 409);
}
