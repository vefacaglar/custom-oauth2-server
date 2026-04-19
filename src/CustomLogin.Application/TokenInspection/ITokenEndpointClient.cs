namespace CustomLogin.Application.TokenInspection;

public interface ITokenEndpointClient
{
    Task<TokenEndpointResponse> ExchangeAuthorizationCodeAsync(
        string tokenEndpoint,
        string clientId,
        string? clientSecret,
        string redirectUri,
        string authorizationCode,
        string? codeVerifier,
        CancellationToken ct = default);

    Task<TokenEndpointResponse> ExecuteClientCredentialsAsync(
        string tokenEndpoint,
        string clientId,
        string clientSecret,
        string? scope,
        CancellationToken ct = default);

    Task<TokenEndpointResponse> RefreshAccessTokenAsync(
        string tokenEndpoint,
        string clientId,
        string? clientSecret,
        string refreshToken,
        CancellationToken ct = default);
}

public sealed class TokenEndpointResponse
{
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public string? IdToken { get; init; }
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; }
    public string? Scope { get; init; }
    public string RawResponse { get; init; } = string.Empty;
    public string? Error { get; init; }
    public string? ErrorDescription { get; init; }
}
