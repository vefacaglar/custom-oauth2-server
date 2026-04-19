using CustomLogin.Application.Dispatcher;
using CustomLogin.Contracts.OAuthFlows;
namespace CustomLogin.Application.OAuthFlows.Commands;

public sealed class HandleOAuthCallbackCommand : ICommand<FlowSessionResponse>
{
    public Guid SessionId { get; set; }
    public string? Code { get; set; }
    public string? State { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}
