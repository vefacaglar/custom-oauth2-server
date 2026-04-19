namespace CustomLogin.Contracts.OAuthFlows;

public sealed class StartAuthorizationCodePkceResponse
{
    public Guid SessionId { get; set; }
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string CodeChallenge { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
