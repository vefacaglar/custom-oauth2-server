namespace CustomLogin.Application.OAuthFlows.Commands;

public sealed class StartAuthorizationCodePkceFlowCommand
{
    public Guid ProviderId { get; set; }
}
