namespace CustomLogin.Contracts.TokenInspection;

public sealed class DecodedJwtResponse
{
    public string Algorithm { get; set; } = string.Empty;
    public string? KeyId { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string? Subject { get; set; }
    public DateTime? Expiration { get; set; }
    public DateTime? IssuedAt { get; set; }
    public DateTime? NotBefore { get; set; }
    public IReadOnlyList<string> Scopes { get; set; } = [];
    public IReadOnlyDictionary<string, object> Claims { get; set; } = new Dictionary<string, object>();
    public string RawHeader { get; set; } = string.Empty;
    public string RawPayload { get; set; } = string.Empty;
    public bool IsValidFormat { get; set; }
    public string? FormatError { get; set; }
    public bool IsExpired { get; set; }
}
