using CustomLogin.Application.Dispatcher;
using CustomLogin.Contracts.OAuthFlows;

namespace CustomLogin.Application.OAuthFlows.Queries;

public sealed class GetFlowSessionByIdQuery : IQuery<FlowSessionResponse>
{
    public Guid Id { get; set; }
}
