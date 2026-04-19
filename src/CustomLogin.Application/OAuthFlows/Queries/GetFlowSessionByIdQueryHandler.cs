using CustomLogin.Application.Dispatcher;

using CustomLogin.Application.OAuthFlows;
using CustomLogin.Contracts.OAuthFlows;
using CustomLogin.Domain;
using CustomLogin.Domain.OAuthFlows;

namespace CustomLogin.Application.OAuthFlows.Queries;

public sealed class GetFlowSessionByIdQueryHandler : IQueryHandler<GetFlowSessionByIdQuery, FlowSessionResponse>
{
    private readonly IFlowSessionRepository _sessionRepository;

    public GetFlowSessionByIdQueryHandler(IFlowSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<FlowSessionResponse>> Handle(GetFlowSessionByIdQuery query, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(query.Id, ct);
        if (session is null)
            return Result<FlowSessionResponse>.Failure("Flow session not found.");

        var response = MapToResponse(session);
        return Result<FlowSessionResponse>.Success(response);
    }

    private static FlowSessionResponse MapToResponse(OAuthFlowSession session)
    {
        return new FlowSessionResponse
        {
            Id = session.Id,
            ProviderId = session.ProviderId,
            FlowType = session.FlowType.ToString(),
            Status = session.Status.ToString(),
            AuthorizationUrl = session.AuthorizationUrl,
            CallbackCode = session.CallbackCode,
            CallbackError = session.CallbackError,
            CreatedAt = session.CreatedAt,
            CompletedAt = session.CompletedAt,
            FailedAt = session.FailedAt
        };
    }
}
