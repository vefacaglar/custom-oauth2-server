using CustomLogin.Domain.TokenInspection;

namespace CustomLogin.Application.TokenInspection;

public interface ITokenResponseRepository
{
    Task<TokenResponseRecord?> GetByIdAsync(TokenResponseId id, CancellationToken ct = default);
    Task<IReadOnlyList<TokenResponseRecord>> GetByFlowSessionIdAsync(Guid flowSessionId, CancellationToken ct = default);
    Task AddAsync(TokenResponseRecord record, CancellationToken ct = default);
    Task<IReadOnlyList<TokenResponseRecord>> ListAsync(CancellationToken ct = default);
}
