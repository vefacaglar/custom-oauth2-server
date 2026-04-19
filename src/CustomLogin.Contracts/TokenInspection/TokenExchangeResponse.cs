namespace CustomLogin.Contracts.TokenInspection;

public sealed class TokenExchangeResponse
{
    public Guid TokenResponseId { get; set; }
    public Guid FlowSessionId { get; set; }
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string? Scope { get; set; }
    public string MaskedAccessToken { get; set; } = string.Empty;
    public string? MaskedRefreshToken { get; set; }
    public string? MaskedIdToken { get; set; }
    public DecodedJwtResponse? DecodedAccessToken { get; set; }
    public DecodedJwtResponse? DecodedIdToken { get; set; }
    public DateTime CreatedAt { get; set; }
}
