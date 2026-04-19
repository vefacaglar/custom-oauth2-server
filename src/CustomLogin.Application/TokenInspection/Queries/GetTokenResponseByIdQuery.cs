using CustomLogin.Application.Dispatcher;
using CustomLogin.Contracts.TokenInspection;

namespace CustomLogin.Application.TokenInspection.Queries;

public sealed class GetTokenResponseByIdQuery : IQuery<TokenResponseSummary>
{
    public Guid Id { get; set; }
}
