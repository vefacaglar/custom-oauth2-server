namespace CustomLogin.Contracts.TokenInspection;

public sealed class TokenResponseSummary
{
    public Guid Id { get; set; }
    public Guid FlowSessionId { get; set; }
    public Guid ProviderId { get; set; }
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string? Scope { get; set; }
    public string MaskedAccessToken { get; set; } = string.Empty;
    public string? MaskedRefreshToken { get; set; }
    public string? MaskedIdToken { get; set; }
    public bool HasAccessToken { get; set; }
    public bool HasRefreshToken { get; set; }
    public bool HasIdToken { get; set; }
    public DateTime CreatedAt { get; set; }
}
