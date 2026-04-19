using CustomLogin.Domain.OAuthFlows;

namespace CustomLogin.Application.OAuthFlows;

public interface IFlowSessionRepository
{
    Task<OAuthFlowSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(OAuthFlowSession session, CancellationToken ct = default);
    Task UpdateAsync(OAuthFlowSession session, CancellationToken ct = default);
    Task<IReadOnlyList<OAuthFlowSession>> ListAsync(CancellationToken ct = default);
}
