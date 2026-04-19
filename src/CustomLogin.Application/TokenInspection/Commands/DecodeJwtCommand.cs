using CustomLogin.Application.Dispatcher;
using CustomLogin.Contracts.TokenInspection;
namespace CustomLogin.Application.TokenInspection.Commands;

public sealed class DecodeJwtCommand : ICommand<DecodedJwtResponse>
{
    public string Token { get; set; } = string.Empty;
}
