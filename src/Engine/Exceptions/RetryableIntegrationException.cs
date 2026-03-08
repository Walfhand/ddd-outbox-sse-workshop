using System.Net;

namespace Engine.Exceptions;

public class RetryableIntegrationException(
    string message,
    string errorCode = "RETRYABLE_INTEGRATION_ERROR",
    HttpStatusCode statusCode = HttpStatusCode.ServiceUnavailable)
    : CustomException(message, errorCode, statusCode);
