using CustomLogin.Domain.ProviderManagement;

namespace CustomLogin.Application.ProviderManagement;

public interface IProviderConfigRepository
{
    Task<OAuthProviderConfig?> GetByIdAsync(ProviderId id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(ProviderName name, CancellationToken ct = default);
    Task AddAsync(OAuthProviderConfig config, CancellationToken ct = default);
    Task UpdateAsync(OAuthProviderConfig config, CancellationToken ct = default);
    Task DeleteAsync(ProviderId id, CancellationToken ct = default);
    Task<IReadOnlyList<OAuthProviderConfig>> ListAsync(CancellationToken ct = default);
}
