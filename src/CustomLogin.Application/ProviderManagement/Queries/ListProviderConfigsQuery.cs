using CustomLogin.Application.Dispatcher;
using CustomLogin.Contracts.ProviderManagement;

namespace CustomLogin.Application.ProviderManagement.Queries;

public sealed class ListProviderConfigsQuery : IQuery<IReadOnlyList<ProviderConfigResponse>>
{
}
