using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using CustomLogin.Domain.OAuthFlows;

namespace CustomLogin.Infrastructure.Persistence.Documents;

public sealed class FlowSessionDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [BsonElement("providerId")]
    public string ProviderId { get; set; } = string.Empty;

    [BsonElement("flowType")]
    public string FlowType { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;

    [BsonElement("pkceVerifier")]
    public string PkceVerifier { get; set; } = string.Empty;

    [BsonElement("pkceChallenge")]
    public string PkceChallenge { get; set; } = string.Empty;

    [BsonElement("state")]
    public string State { get; set; } = string.Empty;

    [BsonElement("authorizationUrl")]
    public string AuthorizationUrl { get; set; } = string.Empty;

    [BsonElement("callbackCode")]
    public string? CallbackCode { get; set; }

    [BsonElement("callbackError")]
    public string? CallbackError { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [BsonElement("failedAt")]
    public DateTime? FailedAt { get; set; }

    public static FlowSessionDocument FromDomain(OAuthFlowSession session)
    {
        return new FlowSessionDocument
        {
            SessionId = session.Id.ToString(),
            ProviderId = session.ProviderId.ToString(),
            FlowType = session.FlowType.ToString(),
            Status = session.Status.ToString(),
            PkceVerifier = session.PkceVerifier,
            PkceChallenge = session.PkceChallenge,
            State = session.State,
            AuthorizationUrl = session.AuthorizationUrl,
            CallbackCode = session.CallbackCode,
            CallbackError = session.CallbackError,
            CreatedAt = session.CreatedAt,
            CompletedAt = session.CompletedAt,
            FailedAt = session.FailedAt
        };
    }

    public OAuthFlowSession ToDomain()
    {
        var events = new List<IDomainEvent>
        {
            new OAuthFlowSessionStartedEvent(
                Guid.Parse(SessionId),
                Guid.Parse(ProviderId),
                Enum.Parse<FlowType>(FlowType),
                CreatedAt)
        };

        if (!string.IsNullOrEmpty(PkceVerifier))
        {
            events.Add(new PkceChallengeGeneratedEvent(
                Guid.Parse(SessionId),
                PkceChallenge,
                PkceVerifier,
                CreatedAt));
        }

        if (!string.IsNullOrEmpty(AuthorizationUrl))
        {
            events.Add(new AuthorizationUrlGeneratedEvent(
                Guid.Parse(SessionId),
                AuthorizationUrl,
                State,
                CreatedAt));
        }

        if (!string.IsNullOrEmpty(CallbackError))
        {
            events.Add(new OAuthFlowSessionFailedEvent(
                Guid.Parse(SessionId),
                CallbackError,
                FailedAt ?? CreatedAt));
        }
        else if (!string.IsNullOrEmpty(CallbackCode))
        {
            events.Add(new OAuthCallbackReceivedEvent(
                Guid.Parse(SessionId),
                CallbackCode,
                State,
                null,
                null,
                CreatedAt));
        }

        if (Status == nameof(FlowSessionStatus.TokenExchangeCompleted))
        {
            events.Add(new AuthorizationCodeExchangedForTokenEvent(
                Guid.Parse(SessionId),
                null, null, null, true, false, false,
                CompletedAt ?? CreatedAt));
        }

        return OAuthFlowSession.RebuildFromEvents(events);
    }
}
