using CustomLogin.Application.Dispatcher;
using CustomLogin.Contracts.ProviderManagement;

namespace CustomLogin.Application.ProviderManagement.Queries;

public sealed class GetProviderConfigByIdQuery : IQuery<ProviderConfigResponse>
{
    public Guid Id { get; set; }
}
