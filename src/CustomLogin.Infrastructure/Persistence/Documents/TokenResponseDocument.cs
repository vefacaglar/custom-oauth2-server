using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using CustomLogin.Domain.TokenInspection;

namespace CustomLogin.Infrastructure.Persistence.Documents;

public sealed class TokenResponseDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("tokenResponseId")]
    public string TokenResponseId { get; set; } = string.Empty;

    [BsonElement("flowSessionId")]
    public string FlowSessionId { get; set; } = string.Empty;

    [BsonElement("providerId")]
    public string ProviderId { get; set; } = string.Empty;

    [BsonElement("accessToken")]
    public string AccessToken { get; set; } = string.Empty;

    [BsonElement("refreshToken")]
    public string? RefreshToken { get; set; }

    [BsonElement("idToken")]
    public string? IdToken { get; set; }

    [BsonElement("tokenType")]
    public string TokenType { get; set; } = string.Empty;

    [BsonElement("expiresIn")]
    public int ExpiresIn { get; set; }

    [BsonElement("scope")]
    public string? Scope { get; set; }

    [BsonElement("rawResponse")]
    public string RawResponse { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    public static TokenResponseDocument FromDomain(TokenResponseRecord record)
    {
        return new TokenResponseDocument
        {
            TokenResponseId = record.Id.Value.ToString(),
            FlowSessionId = record.FlowSessionId.ToString(),
            ProviderId = record.ProviderId.ToString(),
            AccessToken = record.AccessToken,
            RefreshToken = record.RefreshToken,
            IdToken = record.IdToken,
            TokenType = record.TokenType,
            ExpiresIn = record.ExpiresIn,
            Scope = record.Scope,
            RawResponse = record.RawResponse,
            CreatedAt = record.CreatedAt
        };
    }

    public TokenResponseRecord ToDomain()
    {
        return new TokenResponseRecord(
            Domain.TokenInspection.TokenResponseId.From(Guid.Parse(TokenResponseId)),
            Guid.Parse(FlowSessionId),
            Guid.Parse(ProviderId),
            AccessToken,
            RefreshToken,
            IdToken,
            TokenType,
            ExpiresIn,
            Scope,
            RawResponse);
    }
}
