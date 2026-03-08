using Engine.Exceptions;

namespace Api.Domain.Exceptions;

public class DomainException(string message, string errorCode) : CustomException(message, errorCode)
{
}