namespace CustomLogin.Contracts.OAuthFlows;

public sealed class HandleOAuthCallbackRequest
{
    public Guid SessionId { get; set; }
    public string? Code { get; set; }
    public string? State { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}
