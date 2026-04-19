using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Http;
using CustomLogin.Application.TokenInspection;

namespace CustomLogin.Infrastructure.OAuthFlows;

public sealed class TokenEndpointClient : ITokenEndpointClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TokenEndpointClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<TokenEndpointResponse> ExchangeAuthorizationCodeAsync(
        string tokenEndpoint,
        string clientId,
        string? clientSecret,
        string redirectUri,
        string authorizationCode,
        string? codeVerifier,
        CancellationToken ct = default)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("client_id", clientId),
        }.Concat(!string.IsNullOrEmpty(clientSecret)
            ? new[] { new KeyValuePair<string, string>("client_secret", clientSecret!) }
            : Array.Empty<KeyValuePair<string, string>>())
         .Concat(!string.IsNullOrEmpty(codeVerifier)
            ? new[] { new KeyValuePair<string, string>("code_verifier", codeVerifier!) }
            : Array.Empty<KeyValuePair<string, string>>())
         .ToList());

        return await SendTokenRequestAsync(tokenEndpoint, content, ct);
    }

    public async Task<TokenEndpointResponse> ExecuteClientCredentialsAsync(
        string tokenEndpoint,
        string clientId,
        string clientSecret,
        string? scope,
        CancellationToken ct = default)
    {
        var parameters = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", clientId),
            new("client_secret", clientSecret)
        };

        if (!string.IsNullOrEmpty(scope))
            parameters.Add(new("scope", scope!));

        var content = new FormUrlEncodedContent(parameters);
        return await SendTokenRequestAsync(tokenEndpoint, content, ct);
    }

    public async Task<TokenEndpointResponse> RefreshAccessTokenAsync(
        string tokenEndpoint,
        string clientId,
        string? clientSecret,
        string refreshToken,
        CancellationToken ct = default)
    {
        var parameters = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "refresh_token"),
            new("refresh_token", refreshToken),
            new("client_id", clientId)
        };

        if (!string.IsNullOrEmpty(clientSecret))
            parameters.Add(new("client_secret", clientSecret!));

        var content = new FormUrlEncodedContent(parameters);
        return await SendTokenRequestAsync(tokenEndpoint, content, ct);
    }

    private async Task<TokenEndpointResponse> SendTokenRequestAsync(
        string tokenEndpoint,
        FormUrlEncodedContent content,
        CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("TokenEndpoint");

        var response = await client.PostAsync(tokenEndpoint, content, ct);
        var rawBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rawBody);
            return new TokenEndpointResponse
            {
                Error = errorResponse?.TryGetValue("error", out var err) == true ? err.GetString() : response.StatusCode.ToString(),
                ErrorDescription = errorResponse?.TryGetValue("error_description", out var desc) == true ? desc.GetString() : null,
                RawResponse = rawBody
            };
        }

        var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rawBody);
        if (tokenResponse is null)
            throw new InvalidOperationException("Failed to parse token response.");

        return new TokenEndpointResponse
        {
            AccessToken = tokenResponse.TryGetValue("access_token", out var at) ? at.GetString() : null,
            RefreshToken = tokenResponse.TryGetValue("refresh_token", out var rt) ? rt.GetString() : null,
            IdToken = tokenResponse.TryGetValue("id_token", out var idt) ? idt.GetString() : null,
            TokenType = tokenResponse.TryGetValue("token_type", out var tt) ? tt.GetString() ?? "Bearer" : "Bearer",
            ExpiresIn = tokenResponse.TryGetValue("expires_in", out var ei) ? ei.GetInt32() : 0,
            Scope = tokenResponse.TryGetValue("scope", out var sc) ? sc.GetString() : null,
            RawResponse = rawBody
        };
    }
}
