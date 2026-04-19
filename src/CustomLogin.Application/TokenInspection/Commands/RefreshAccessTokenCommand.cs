using CustomLogin.Application.Dispatcher;
using CustomLogin.Contracts.TokenInspection;
namespace CustomLogin.Application.TokenInspection.Commands;

public sealed class RefreshAccessTokenCommand : ICommand<TokenExchangeResponse>
{
    public Guid FlowSessionId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}
