using CustomLogin.Domain.OAuthFlows;

namespace CustomLogin.Application.OAuthFlows;

public interface IEventStore
{
    Task AppendEventsAsync(Guid streamId, string streamType, IEnumerable<IDomainEvent> events, CancellationToken ct = default);
    Task<IReadOnlyList<IDomainEvent>> GetEventsAsync(Guid streamId, CancellationToken ct = default);
}
