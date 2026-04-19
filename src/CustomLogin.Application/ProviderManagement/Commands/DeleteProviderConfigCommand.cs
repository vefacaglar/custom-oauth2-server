using CustomLogin.Application.Dispatcher;

namespace CustomLogin.Application.ProviderManagement.Commands;

public sealed class DeleteProviderConfigCommand : ICommand
{
    public Guid Id { get; set; }
}
