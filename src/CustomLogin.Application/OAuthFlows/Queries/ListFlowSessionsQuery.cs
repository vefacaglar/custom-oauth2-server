using CustomLogin.Application.Dispatcher;
using CustomLogin.Contracts.OAuthFlows;

namespace CustomLogin.Application.OAuthFlows.Queries;

public sealed class ListFlowSessionsQuery : IQuery<IReadOnlyList<FlowSessionResponse>>
{
}
