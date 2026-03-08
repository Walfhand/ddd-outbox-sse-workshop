using System.Net;

namespace Engine.Exceptions;

public class NotFoundException : CustomException
{
    public NotFoundException(string message) : base(message, "NOT_FOUND", HttpStatusCode.NotFound)
    {
    }

    public NotFoundException() : base("", "NOT_FOUND", HttpStatusCode.NotFound)
    {
    }
}

public static class ObjectExtensions
{
    public static NotFoundException NotFound(this object? obj, string message)
    {
        return new NotFoundException(message);
    }
}