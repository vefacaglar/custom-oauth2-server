using CustomLogin.Application.OAuthFlows;
using CustomLogin.Contracts.OAuthFlows;
using CustomLogin.Domain;
using CustomLogin.Domain.OAuthFlows;

namespace CustomLogin.Application.OAuthFlows.Queries;

public sealed class ListFlowSessionsQueryHandler
{
    private readonly IFlowSessionRepository _sessionRepository;

    public ListFlowSessionsQueryHandler(IFlowSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<IReadOnlyList<FlowSessionResponse>>> Handle(ListFlowSessionsQuery query, CancellationToken ct = default)
    {
        var sessions = await _sessionRepository.ListAsync(ct);
        var responses = sessions.Select(MapToResponse).ToList();
        return Result<IReadOnlyList<FlowSessionResponse>>.Success(responses);
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
