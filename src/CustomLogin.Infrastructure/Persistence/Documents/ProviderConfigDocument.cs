using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using CustomLogin.Domain.ProviderManagement;

namespace CustomLogin.Infrastructure.Persistence.Documents;

public sealed class ProviderConfigDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("providerId")]
    public string ProviderId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("authorizationEndpoint")]
    public string AuthorizationEndpoint { get; set; } = string.Empty;

    [BsonElement("tokenEndpoint")]
    public string TokenEndpoint { get; set; } = string.Empty;

    [BsonElement("revocationEndpoint")]
    public string? RevocationEndpoint { get; set; }

    [BsonElement("introspectionEndpoint")]
    public string? IntrospectionEndpoint { get; set; }

    [BsonElement("userInfoEndpoint")]
    public string? UserInfoEndpoint { get; set; }

    [BsonElement("issuer")]
    public string? Issuer { get; set; }

    [BsonElement("clientId")]
    public string ClientId { get; set; } = string.Empty;

    [BsonElement("clientSecret")]
    public string? ClientSecret { get; set; }

    [BsonElement("redirectUri")]
    public string RedirectUri { get; set; } = string.Empty;

    [BsonElement("defaultScopes")]
    public List<string> DefaultScopes { get; set; } = [];

    [BsonElement("supportedGrantTypes")]
    public List<string> SupportedGrantTypes { get; set; } = [];

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    public static ProviderConfigDocument FromDomain(OAuthProviderConfig config)
    {
        return new ProviderConfigDocument
        {
            ProviderId = config.Id.Value.ToString(),
            Name = config.Name.Value,
            AuthorizationEndpoint = config.AuthorizationEndpoint.Value.ToString(),
            TokenEndpoint = config.TokenEndpoint.Value.ToString(),
            RevocationEndpoint = config.RevocationEndpoint?.Value.ToString(),
            IntrospectionEndpoint = config.IntrospectionEndpoint?.Value.ToString(),
            UserInfoEndpoint = config.UserInfoEndpoint?.Value.ToString(),
            Issuer = config.Issuer?.Value.ToString(),
            ClientId = config.ClientId.Value,
            ClientSecret = config.ClientSecret?.Value,
            RedirectUri = config.RedirectUri.Value.ToString(),
            DefaultScopes = config.DefaultScopes.Scopes.ToList(),
            SupportedGrantTypes = config.SupportedGrantTypes.Select(g => g.Value).ToList(),
            CreatedAt = config.CreatedAt,
            UpdatedAt = config.UpdatedAt
        };
    }

    public OAuthProviderConfig ToDomain()
    {
        var providerId = Domain.ProviderManagement.ProviderId.From(Guid.Parse(ProviderId));
        var config = new OAuthProviderConfig(
            providerId,
            Domain.ProviderManagement.ProviderName.From(Name),
            Domain.ProviderManagement.OAuthEndpoint.From(AuthorizationEndpoint),
            Domain.ProviderManagement.OAuthEndpoint.From(TokenEndpoint),
            Domain.ProviderManagement.ClientId.From(ClientId),
            Domain.ProviderManagement.RedirectUri.From(RedirectUri));

        Domain.ProviderManagement.ClientSecret? clientSecret = null;
        if (!string.IsNullOrWhiteSpace(ClientSecret))
            clientSecret = Domain.ProviderManagement.ClientSecret.From(ClientSecret);

        config.Update(
            Domain.ProviderManagement.ProviderName.From(Name),
            Domain.ProviderManagement.OAuthEndpoint.From(AuthorizationEndpoint),
            Domain.ProviderManagement.OAuthEndpoint.From(TokenEndpoint),
            Domain.ProviderManagement.ClientId.From(ClientId),
            Domain.ProviderManagement.RedirectUri.From(RedirectUri),
            clientSecret,
            Domain.ProviderManagement.ScopeCollection.From(DefaultScopes),
            SupportedGrantTypes.Select(Domain.ProviderManagement.GrantType.From).ToList());

        config.SetOptionalEndpoints(
            RevocationEndpoint is not null ? Domain.ProviderManagement.OAuthEndpoint.From(RevocationEndpoint) : null,
            IntrospectionEndpoint is not null ? Domain.ProviderManagement.OAuthEndpoint.From(IntrospectionEndpoint) : null,
            UserInfoEndpoint is not null ? Domain.ProviderManagement.OAuthEndpoint.From(UserInfoEndpoint) : null,
            Issuer is not null ? Domain.ProviderManagement.OAuthEndpoint.From(Issuer) : null);

        return config;
    }
}
