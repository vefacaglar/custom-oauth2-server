using CustomLogin.Application.Dispatcher;
using CustomLogin.Contracts.TokenInspection;
namespace CustomLogin.Application.TokenInspection.Commands;

public sealed class ExecuteClientCredentialsCommand : ICommand<TokenExchangeResponse>
{
    public Guid ProviderId { get; set; }
    public string? Scope { get; set; }
}
