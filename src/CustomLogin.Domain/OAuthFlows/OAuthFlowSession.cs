using CustomLogin.Domain.OAuthFlows;

namespace CustomLogin.Domain.OAuthFlows;

public sealed class OAuthFlowSession
{
    private readonly List<IDomainEvent> _uncommittedEvents = [];
    private string _pkceVerifier = string.Empty;
    private string _pkceChallenge = string.Empty;
    private string _state = string.Empty;
    private string _authorizationUrl = string.Empty;
    private string? _callbackCode;
    private string? _callbackError;

    public Guid Id { get; private set; }
    public Guid ProviderId { get; private set; }
    public FlowType FlowType { get; private set; }
    public FlowSessionStatus Status { get; private set; }
    public string PkceVerifier => _pkceVerifier;
    public string PkceChallenge => _pkceChallenge;
    public string State => _state;
    public string AuthorizationUrl => _authorizationUrl;
    public string? CallbackCode => _callbackCode;
    public string? CallbackError => _callbackError;
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }

    public IReadOnlyList<IDomainEvent> UncommittedEvents => _uncommittedEvents;

    private OAuthFlowSession() { }

    public static OAuthFlowSession Create(Guid providerId, FlowType flowType)
    {
        var session = new OAuthFlowSession
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            FlowType = flowType,
            Status = FlowSessionStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        var startedEvent = new OAuthFlowSessionStartedEvent(session.Id, providerId, flowType, session.CreatedAt);
        session._uncommittedEvents.Add(startedEvent);

        return session;
    }

    public void GeneratePkceChallenge(string codeVerifier, string codeChallenge)
    {
        if (Status != FlowSessionStatus.Created)
            throw new InvalidOperationException("PKCE can only be generated in Created state.");

        _pkceVerifier = codeVerifier;
        _pkceChallenge = codeChallenge;

        var pkceEvent = new PkceChallengeGeneratedEvent(Id, codeChallenge, codeVerifier, DateTime.UtcNow);
        _uncommittedEvents.Add(pkceEvent);
    }

    public void GenerateAuthorizationUrl(string url, string state)
    {
        if (string.IsNullOrEmpty(_pkceVerifier))
            throw new InvalidOperationException("PKCE verifier must be generated before authorization URL.");

        _authorizationUrl = url;
        _state = state;
        Status = FlowSessionStatus.AuthorizationUrlGenerated;

        var urlEvent = new AuthorizationUrlGeneratedEvent(Id, url, state, DateTime.UtcNow);
        _uncommittedEvents.Add(urlEvent);
    }

    public void ReceiveCallback(string? code, string? state, string? error, string? errorDescription)
    {
        if (Status != FlowSessionStatus.AuthorizationUrlGenerated)
            throw new InvalidOperationException("Callback can only be received after authorization URL is generated.");

        if (!string.IsNullOrEmpty(state) && _state != state)
            throw new InvalidOperationException("State mismatch. Expected callback state to match stored state.");

        if (!string.IsNullOrEmpty(error))
        {
            _callbackError = error;
            Status = FlowSessionStatus.Failed;
            FailedAt = DateTime.UtcNow;

            var failedEvent = new OAuthFlowSessionFailedEvent(Id, error, DateTime.UtcNow);
            _uncommittedEvents.Add(failedEvent);
        }
        else if (!string.IsNullOrEmpty(code))
        {
            _callbackCode = code;
            Status = FlowSessionStatus.CallbackReceived;

            var callbackEvent = new OAuthCallbackReceivedEvent(Id, code, state, null, null, DateTime.UtcNow);
            _uncommittedEvents.Add(callbackEvent);
        }
        else
        {
            throw new InvalidOperationException("Callback must contain either an authorization code or an error.");
        }
    }

    public void MarkTokenExchangeCompleted(string? tokenType, int? expiresIn, string? scope, bool hasAccessToken, bool hasRefreshToken, bool hasIdToken)
    {
        if (Status != FlowSessionStatus.CallbackReceived)
            throw new InvalidOperationException("Token exchange can only occur after callback is received.");

        Status = FlowSessionStatus.TokenExchangeCompleted;
        CompletedAt = DateTime.UtcNow;

        var exchangeEvent = new AuthorizationCodeExchangedForTokenEvent(Id, tokenType, expiresIn, scope, hasAccessToken, hasRefreshToken, hasIdToken, DateTime.UtcNow);
        _uncommittedEvents.Add(exchangeEvent);
    }

    public void MarkFailed(string reason)
    {
        Status = FlowSessionStatus.Failed;
        FailedAt = DateTime.UtcNow;

        var failedEvent = new OAuthFlowSessionFailedEvent(Id, reason, DateTime.UtcNow);
        _uncommittedEvents.Add(failedEvent);
    }

    public void MarkExpired()
    {
        if (Status is FlowSessionStatus.TokenExchangeCompleted or FlowSessionStatus.Failed)
            throw new InvalidOperationException("Completed or failed sessions cannot be expired.");

        Status = FlowSessionStatus.Expired;

        var expiredEvent = new OAuthFlowSessionExpiredEvent(Id, DateTime.UtcNow);
        _uncommittedEvents.Add(expiredEvent);
    }

    public void ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
    }

    public static OAuthFlowSession RebuildFromEvents(IEnumerable<IDomainEvent> events)
    {
        var session = new OAuthFlowSession();

        foreach (var e in events)
        {
            session.ApplyEvent(e);
        }

        return session;
    }

    private void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case OAuthFlowSessionStartedEvent started:
                Id = started.StreamId;
                ProviderId = started.ProviderId;
                FlowType = started.FlowType;
                Status = FlowSessionStatus.Created;
                CreatedAt = started.OccurredAt;
                break;

            case PkceChallengeGeneratedEvent pkce:
                _pkceVerifier = pkce.CodeVerifier;
                _pkceChallenge = pkce.CodeChallenge;
                break;

            case AuthorizationUrlGeneratedEvent url:
                _authorizationUrl = url.AuthorizationUrl;
                _state = url.State;
                Status = FlowSessionStatus.AuthorizationUrlGenerated;
                break;

            case OAuthCallbackReceivedEvent callback:
                if (!string.IsNullOrEmpty(callback.Error))
                {
                    _callbackError = callback.Error;
                    Status = FlowSessionStatus.Failed;
                    FailedAt = callback.OccurredAt;
                }
                else if (!string.IsNullOrEmpty(callback.AuthorizationCode))
                {
                    _callbackCode = callback.AuthorizationCode;
                    Status = FlowSessionStatus.CallbackReceived;
                }
                break;

            case AuthorizationCodeExchangedForTokenEvent:
                Status = FlowSessionStatus.TokenExchangeCompleted;
                CompletedAt = domainEvent.OccurredAt;
                break;

            case OAuthFlowSessionFailedEvent failed:
                Status = FlowSessionStatus.Failed;
                FailedAt = failed.OccurredAt;
                break;

            case OAuthFlowSessionExpiredEvent:
                Status = FlowSessionStatus.Expired;
                break;
        }
    }
}
