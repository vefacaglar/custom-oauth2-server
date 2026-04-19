namespace CustomLogin.Contracts.TokenInspection;

public sealed class ExchangeAuthorizationCodeRequest
{
    public Guid FlowSessionId { get; set; }
}
