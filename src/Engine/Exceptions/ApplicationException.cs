using System.Net;

namespace Engine.Exceptions;

public class ApplicationException(
    string message,
    string errorCode = "APPLICATION_ERROR",
    HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    : CustomException(message, errorCode, statusCode);