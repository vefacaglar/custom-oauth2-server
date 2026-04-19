namespace CustomLogin.Domain.TokenInspection;

public sealed class TokenResponseRecord
{
    public TokenResponseId Id { get; private set; }
    public Guid FlowSessionId { get; private set; }
    public Guid ProviderId { get; private set; }
    public string AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public string? IdToken { get; private set; }
    public string TokenType { get; private set; }
    public int ExpiresIn { get; private set; }
    public string? Scope { get; private set; }
    public string RawResponse { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private TokenResponseRecord()
    {
        Id = null!;
        AccessToken = null!;
        TokenType = null!;
        RawResponse = null!;
    }

    public TokenResponseRecord(
        TokenResponseId id,
        Guid flowSessionId,
        Guid providerId,
        string accessToken,
        string? refreshToken,
        string? idToken,
        string tokenType,
        int expiresIn,
        string? scope,
        string rawResponse)
    {
        Id = id;
        FlowSessionId = flowSessionId;
        ProviderId = providerId;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        IdToken = idToken;
        TokenType = tokenType;
        ExpiresIn = expiresIn;
        Scope = scope;
        RawResponse = rawResponse;
        CreatedAt = DateTime.UtcNow;
    }

    public string MaskAccessToken()
    {
        if (string.IsNullOrEmpty(AccessToken))
            return string.Empty;

        if (AccessToken.Length <= 12)
            return "***";

        return AccessToken[..8] + "..." + AccessToken[^4..];
    }

    public string MaskRefreshToken()
    {
        if (string.IsNullOrEmpty(RefreshToken))
            return string.Empty;

        if (RefreshToken.Length <= 12)
            return "***";

        return RefreshToken[..8] + "..." + RefreshToken[^4..];
    }

    public string MaskIdToken()
    {
        if (string.IsNullOrEmpty(IdToken))
            return string.Empty;

        if (IdToken.Length <= 12)
            return "***";

        return IdToken[..8] + "..." + IdToken[^4..];
    }
}
