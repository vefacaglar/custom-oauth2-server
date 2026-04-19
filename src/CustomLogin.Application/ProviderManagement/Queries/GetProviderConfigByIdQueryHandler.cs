using CustomLogin.Application.Dispatcher;

using CustomLogin.Contracts.ProviderManagement;
using CustomLogin.Domain;
using CustomLogin.Domain.ProviderManagement;

namespace CustomLogin.Application.ProviderManagement.Queries;

public sealed class GetProviderConfigByIdQueryHandler : IQueryHandler<GetProviderConfigByIdQuery, ProviderConfigResponse>
{
    private readonly IProviderConfigRepository _repository;

    public GetProviderConfigByIdQueryHandler(IProviderConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProviderConfigResponse>> Handle(GetProviderConfigByIdQuery query, CancellationToken ct = default)
    {
        var id = ProviderId.From(query.Id);
        var config = await _repository.GetByIdAsync(id, ct);

        if (config is null)
            return Result<ProviderConfigResponse>.Failure("Provider config not found.");

        var response = MapToResponse(config);
        return Result<ProviderConfigResponse>.Success(response);
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
