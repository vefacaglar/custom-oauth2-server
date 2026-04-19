namespace CustomLogin.Domain.ProviderManagement;

public sealed class OAuthProviderConfig
{
    public ProviderId Id { get; private set; }
    public ProviderName Name { get; private set; }
    public OAuthEndpoint AuthorizationEndpoint { get; private set; }
    public OAuthEndpoint TokenEndpoint { get; private set; }
    public OAuthEndpoint? RevocationEndpoint { get; private set; }
    public OAuthEndpoint? IntrospectionEndpoint { get; private set; }
    public OAuthEndpoint? UserInfoEndpoint { get; private set; }
    public OAuthEndpoint? Issuer { get; private set; }
    public ClientId ClientId { get; private set; }
    public ClientSecret? ClientSecret { get; private set; }
    public RedirectUri RedirectUri { get; private set; }
    public ScopeCollection DefaultScopes { get; private set; }
    public IReadOnlyList<GrantType> SupportedGrantTypes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private OAuthProviderConfig()
    {
        Id = null!;
        Name = null!;
        AuthorizationEndpoint = null!;
        TokenEndpoint = null!;
        ClientId = null!;
        RedirectUri = null!;
        SupportedGrantTypes = [];
        DefaultScopes = ScopeCollection.Empty();
    }

    public OAuthProviderConfig(
        ProviderId id,
        ProviderName name,
        OAuthEndpoint authorizationEndpoint,
        OAuthEndpoint tokenEndpoint,
        ClientId clientId,
        RedirectUri redirectUri)
    {
        Id = id;
        Name = name;
        AuthorizationEndpoint = authorizationEndpoint;
        TokenEndpoint = tokenEndpoint;
        ClientId = clientId;
        RedirectUri = redirectUri;
        DefaultScopes = ScopeCollection.Empty();
        SupportedGrantTypes = [];
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        ProviderName name,
        OAuthEndpoint authorizationEndpoint,
        OAuthEndpoint tokenEndpoint,
        ClientId clientId,
        RedirectUri redirectUri,
        ClientSecret? clientSecret,
        ScopeCollection defaultScopes,
        IReadOnlyList<GrantType> supportedGrantTypes)
    {
        Name = name;
        AuthorizationEndpoint = authorizationEndpoint;
        TokenEndpoint = tokenEndpoint;
        ClientId = clientId;
        RedirectUri = redirectUri;
        ClientSecret = clientSecret;
        DefaultScopes = defaultScopes;
        SupportedGrantTypes = supportedGrantTypes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOptionalEndpoints(
        OAuthEndpoint? revocationEndpoint,
        OAuthEndpoint? introspectionEndpoint,
        OAuthEndpoint? userInfoEndpoint,
        OAuthEndpoint? issuer)
    {
        RevocationEndpoint = revocationEndpoint;
        IntrospectionEndpoint = introspectionEndpoint;
        UserInfoEndpoint = userInfoEndpoint;
        Issuer = issuer;
    }
}
