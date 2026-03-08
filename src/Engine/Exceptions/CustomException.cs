using System.Net;

namespace Engine.Exceptions;

public abstract class CustomException : Exception
{
    protected CustomException(
        string message,
        string errorCode,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest) : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public string ErrorCode { get; }
    public HttpStatusCode StatusCode { get; }
}