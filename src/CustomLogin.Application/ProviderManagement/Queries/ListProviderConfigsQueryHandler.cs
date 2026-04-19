using CustomLogin.Contracts.ProviderManagement;
using CustomLogin.Domain;
using CustomLogin.Domain.ProviderManagement;

namespace CustomLogin.Application.ProviderManagement.Queries;

public sealed class ListProviderConfigsQueryHandler
{
    private readonly IProviderConfigRepository _repository;

    public ListProviderConfigsQueryHandler(IProviderConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ProviderConfigResponse>>> Handle(ListProviderConfigsQuery query, CancellationToken ct = default)
    {
        var configs = await _repository.ListAsync(ct);

        var responses = configs.Select(MapToResponse).ToList();
        return Result<IReadOnlyList<ProviderConfigResponse>>.Success(responses);
    }

    private static ProviderConfigResponse MapToResponse(OAuthProviderConfig config)
    {
        return new ProviderConfigResponse
        {
            Id = config.Id.Value,
            Name = config.Name.Value,
            AuthorizationEndpoint = config.AuthorizationEndpoint.Value.ToString(),
            TokenEndpoint = config.TokenEndpoint.Value.ToString(),
            RevocationEndpoint = config.RevocationEndpoint?.Value.ToString(),
            IntrospectionEndpoint = config.IntrospectionEndpoint?.Value.ToString(),
            UserInfoEndpoint = config.UserInfoEndpoint?.Value.ToString(),
            Issuer = config.Issuer?.Value.ToString(),
            ClientId = config.ClientId.Value,
            HasClientSecret = config.ClientSecret is not null,
            RedirectUri = config.RedirectUri.Value.ToString(),
            DefaultScopes = config.DefaultScopes.Scopes,
            SupportedGrantTypes = config.SupportedGrantTypes.Select(g => g.Value).ToList(),
            CreatedAt = config.CreatedAt,
            UpdatedAt = config.UpdatedAt
        };
    }
}
