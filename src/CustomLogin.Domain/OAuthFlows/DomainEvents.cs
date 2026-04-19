namespace CustomLogin.Domain.OAuthFlows;

public interface IDomainEvent
{
    Guid StreamId { get; }
    DateTime OccurredAt { get; }
}

public sealed record OAuthFlowSessionStartedEvent : IDomainEvent
{
    public Guid StreamId { get; }
    public Guid ProviderId { get; }
    public FlowType FlowType { get; }
    public DateTime OccurredAt { get; }

    public OAuthFlowSessionStartedEvent(Guid streamId, Guid providerId, FlowType flowType, DateTime occurredAt)
    {
        StreamId = streamId;
        ProviderId = providerId;
        FlowType = flowType;
        OccurredAt = occurredAt;
    }
}

public sealed record PkceChallengeGeneratedEvent : IDomainEvent
{
    public Guid StreamId { get; }
    public string CodeChallenge { get; }
    public string CodeVerifier { get; }
    public DateTime OccurredAt { get; }

    public PkceChallengeGeneratedEvent(Guid streamId, string codeChallenge, string codeVerifier, DateTime occurredAt)
    {
        StreamId = streamId;
        CodeChallenge = codeChallenge;
        CodeVerifier = codeVerifier;
        OccurredAt = occurredAt;
    }
}

public sealed record AuthorizationUrlGeneratedEvent : IDomainEvent
{
    public Guid StreamId { get; }
    public string AuthorizationUrl { get; }
    public string State { get; }
    public DateTime OccurredAt { get; }

    public AuthorizationUrlGeneratedEvent(Guid streamId, string authorizationUrl, string state, DateTime occurredAt)
    {
        StreamId = streamId;
        AuthorizationUrl = authorizationUrl;
        State = state;
        OccurredAt = occurredAt;
    }
}

public sealed record OAuthCallbackReceivedEvent : IDomainEvent
{
    public Guid StreamId { get; }
    public string? AuthorizationCode { get; }
    public string? State { get; }
    public string? Error { get; }
    public string? ErrorDescription { get; }
    public DateTime OccurredAt { get; }

    public OAuthCallbackReceivedEvent(Guid streamId, string? authorizationCode, string? state, string? error, string? errorDescription, DateTime occurredAt)
    {
        StreamId = streamId;
        AuthorizationCode = authorizationCode;
        State = state;
        Error = error;
        ErrorDescription = errorDescription;
        OccurredAt = occurredAt;
    }
}

public sealed record AuthorizationCodeExchangedForTokenEvent : IDomainEvent
{
    public Guid StreamId { get; }
    public string? TokenType { get; }
    public int? ExpiresIn { get; }
    public string? Scope { get; }
    public bool HasAccessToken { get; }
    public bool HasRefreshToken { get; }
    public bool HasIdToken { get; }
    public DateTime OccurredAt { get; }

    public AuthorizationCodeExchangedForTokenEvent(Guid streamId, string? tokenType, int? expiresIn, string? scope, bool hasAccessToken, bool hasRefreshToken, bool hasIdToken, DateTime occurredAt)
    {
        StreamId = streamId;
        TokenType = tokenType;
        ExpiresIn = expiresIn;
        Scope = scope;
        HasAccessToken = hasAccessToken;
        HasRefreshToken = hasRefreshToken;
        HasIdToken = hasIdToken;
        OccurredAt = occurredAt;
    }
}

public sealed record OAuthFlowSessionFailedEvent : IDomainEvent
{
    public Guid StreamId { get; }
    public string Reason { get; }
    public DateTime OccurredAt { get; }

    public OAuthFlowSessionFailedEvent(Guid streamId, string reason, DateTime occurredAt)
    {
        StreamId = streamId;
        Reason = reason;
        OccurredAt = occurredAt;
    }
}

public sealed record OAuthFlowSessionExpiredEvent : IDomainEvent
{
    public Guid StreamId { get; }
    public DateTime OccurredAt { get; }

    public OAuthFlowSessionExpiredEvent(Guid streamId, DateTime occurredAt)
    {
        StreamId = streamId;
        OccurredAt = occurredAt;
    }
}
