using CustomLogin.Application.Dispatcher;
using CustomLogin.Contracts.OAuthFlows;
namespace CustomLogin.Application.OAuthFlows.Commands;

public sealed class StartAuthorizationCodePkceFlowCommand : ICommand<StartAuthorizationCodePkceResponse>
{
    public Guid ProviderId { get; set; }
}
