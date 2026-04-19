namespace CustomLogin.Application.TokenInspection.Commands;

public sealed class ExchangeAuthorizationCodeCommand
{
    public Guid FlowSessionId { get; set; }
}
