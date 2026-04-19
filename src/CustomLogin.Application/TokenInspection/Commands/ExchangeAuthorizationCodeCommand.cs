using CustomLogin.Application.Dispatcher;
using CustomLogin.Contracts.TokenInspection;
namespace CustomLogin.Application.TokenInspection.Commands;

public sealed class ExchangeAuthorizationCodeCommand : ICommand<TokenExchangeResponse>
{
    public Guid FlowSessionId { get; set; }
}
