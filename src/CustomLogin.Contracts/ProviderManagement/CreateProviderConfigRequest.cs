namespace CustomLogin.Contracts.ProviderManagement;

public sealed class CreateProviderConfigRequest
{
    public string Name { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string? RevocationEndpoint { get; set; }
    public string? IntrospectionEndpoint { get; set; }
    public string? UserInfoEndpoint { get; set; }
    public string? Issuer { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public string RedirectUri { get; set; } = string.Empty;
    public List<string> DefaultScopes { get; set; } = [];
    public List<string> SupportedGrantTypes { get; set; } = [];
}
