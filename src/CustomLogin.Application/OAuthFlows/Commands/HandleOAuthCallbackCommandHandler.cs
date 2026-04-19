using CustomLogin.Application.OAuthFlows;
using CustomLogin.Contracts.OAuthFlows;
using CustomLogin.Domain;
using CustomLogin.Domain.OAuthFlows;

namespace CustomLogin.Application.OAuthFlows.Commands;

public sealed class HandleOAuthCallbackCommandHandler
{
    private readonly IFlowSessionRepository _sessionRepository;
    private readonly IEventStore _eventStore;

    public HandleOAuthCallbackCommandHandler(IFlowSessionRepository sessionRepository, IEventStore eventStore)
    {
        _sessionRepository = sessionRepository;
        _eventStore = eventStore;
    }

    public async Task<Result<FlowSessionResponse>> Handle(HandleOAuthCallbackCommand command, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(command.SessionId, ct);
        if (session is null)
            return Result<FlowSessionResponse>.Failure("Flow session not found.");

        try
        {
            session.ReceiveCallback(command.Code, command.State, command.Error, command.ErrorDescription);
        }
        catch (InvalidOperationException ex)
        {
            return Result<FlowSessionResponse>.Failure(ex.Message);
        }

        await _sessionRepository.UpdateAsync(session, ct);
        await _eventStore.AppendEventsAsync(session.Id, nameof(OAuthFlowSession), session.UncommittedEvents, ct);
        session.ClearUncommittedEvents();

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
