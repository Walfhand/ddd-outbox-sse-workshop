using Engine.Exceptions;

namespace Api.Features.Notifications.Shared;

public sealed class SseTargetOfflineException(string message)
    : RetryableIntegrationException(message, "SSE_TARGET_OFFLINE");
