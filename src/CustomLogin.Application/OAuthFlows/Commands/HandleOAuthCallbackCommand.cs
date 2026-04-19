namespace CustomLogin.Application.OAuthFlows.Commands;

public sealed class HandleOAuthCallbackCommand
{
    public Guid SessionId { get; set; }
    public string? Code { get; set; }
    public string? State { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}
