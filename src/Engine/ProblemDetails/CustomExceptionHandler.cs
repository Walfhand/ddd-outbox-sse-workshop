using Engine.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Engine.ProblemDetails;

internal sealed class CustomExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var handledException = exception is CustomException or ValidationException
            ? exception
            : exception.GetBaseException();

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = handledException switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                CustomException customException => (int)customException.StatusCode,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            },
            Title = "An error occurred",
            Type = handledException.GetType().Name,
            Detail = handledException.Message
        };
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        if (handledException is not ValidationException validationException)
        {
            if (handledException is CustomException customException)
                problemDetails.Extensions["errorCode"] = customException.ErrorCode;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                Exception = handledException,
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });
        }

        var validationErrors = validationException.Errors
            .Select(failure => new
            {
                PropertyName = failure.PropertyName.Split('.').Last(),
                failure.ErrorMessage,
                failure.ErrorCode
            })
            .ToArray();

        problemDetails.Extensions["validationErrors"] = validationErrors;

        var firstErrorCode = validationErrors
            .Select(x => x.ErrorCode)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        if (!string.IsNullOrWhiteSpace(firstErrorCode))
            problemDetails.Extensions["errorCode"] = firstErrorCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            Exception = handledException,
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }
}