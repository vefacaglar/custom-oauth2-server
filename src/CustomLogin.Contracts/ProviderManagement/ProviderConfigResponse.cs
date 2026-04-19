namespace CustomLogin.Contracts.ProviderManagement;

public sealed class ProviderConfigResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string? RevocationEndpoint { get; set; }
    public string? IntrospectionEndpoint { get; set; }
    public string? UserInfoEndpoint { get; set; }
    public string? Issuer { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public bool HasClientSecret { get; set; }
    public string RedirectUri { get; set; } = string.Empty;
    public IReadOnlyList<string> DefaultScopes { get; set; } = [];
    public IReadOnlyList<string> SupportedGrantTypes { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
