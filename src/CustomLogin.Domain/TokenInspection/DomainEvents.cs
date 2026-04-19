namespace CustomLogin.Domain.TokenInspection;

public sealed record TokenExchangeCompletedEvent(
    Guid FlowSessionId,
    Guid ProviderId,
    string TokenType,
    int ExpiresIn,
    string? Scope,
    bool HasAccessToken,
    bool HasRefreshToken,
    bool HasIdToken,
    DateTime OccurredAt);

public sealed record TokenDecodedEvent(
    string Token,
    string TokenType,
    bool IsValidFormat,
    DateTime OccurredAt);
