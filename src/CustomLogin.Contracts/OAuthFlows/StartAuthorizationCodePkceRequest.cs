namespace CustomLogin.Contracts.OAuthFlows;

public sealed class StartAuthorizationCodePkceRequest
{
    public Guid ProviderId { get; set; }
}
