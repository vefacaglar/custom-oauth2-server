using OAuthLab.Application.ProviderManagement;
using OAuthLab.Domain;
using OAuthLab.Domain.ProviderManagement;

namespace OAuthLab.Application.ProviderManagement.Commands;

public sealed class CreateProviderConfigCommandHandler
{
    private readonly IProviderConfigRepository _repository;

    public CreateProviderConfigCommandHandler(IProviderConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateProviderConfigCommand command, CancellationToken ct = default)
    {
        var name = ProviderName.From(command.Name);

        if (await _repository.ExistsByNameAsync(name, ct))
            return Result<Guid>.Failure($"Provider with name '{command.Name}' already exists.");

        var id = ProviderId.New();
        var authorizationEndpoint = OAuthEndpoint.From(command.AuthorizationEndpoint);
        var tokenEndpoint = OAuthEndpoint.From(command.TokenEndpoint);
        var clientId = ClientId.From(command.ClientId);
        var redirectUri = RedirectUri.From(command.RedirectUri);

        var config = new OAuthProviderConfig(
            id, name, authorizationEndpoint, tokenEndpoint, clientId, redirectUri);

        ClientSecret? clientSecret = null;
        if (!string.IsNullOrWhiteSpace(command.ClientSecret))
            clientSecret = ClientSecret.From(command.ClientSecret);

        config.Update(name, authorizationEndpoint, tokenEndpoint, clientId, redirectUri,
            clientSecret, ScopeCollection.From(command.DefaultScopes),
            command.SupportedGrantTypes.Select(GrantType.From).ToList());

        var optionalEndpoints = command.RevocationEndpoint is not null
            ? OAuthEndpoint.From(command.RevocationEndpoint)
            : null;

        config.SetOptionalEndpoints(
            optionalEndpoints,
            command.IntrospectionEndpoint is not null ? OAuthEndpoint.From(command.IntrospectionEndpoint) : null,
            command.UserInfoEndpoint is not null ? OAuthEndpoint.From(command.UserInfoEndpoint) : null,
            command.Issuer is not null ? OAuthEndpoint.From(command.Issuer) : null);

        await _repository.AddAsync(config, ct);

        return Result<Guid>.Success(id.Value);
    }
}
