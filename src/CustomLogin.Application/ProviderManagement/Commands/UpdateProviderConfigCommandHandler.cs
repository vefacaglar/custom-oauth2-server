using CustomLogin.Application.Dispatcher;

using CustomLogin.Domain;
using CustomLogin.Domain.ProviderManagement;

namespace CustomLogin.Application.ProviderManagement.Commands;

public sealed class UpdateProviderConfigCommandHandler : ICommandHandler<UpdateProviderConfigCommand>
{
    private readonly IProviderConfigRepository _repository;

    public UpdateProviderConfigCommandHandler(IProviderConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateProviderConfigCommand command, CancellationToken ct = default)
    {
        var id = ProviderId.From(command.Id);
        var config = await _repository.GetByIdAsync(id, ct);

        if (config is null)
            return Result.Failure("Provider config not found.");

        var name = ProviderName.From(command.Name);

        var existingByName = await _repository.GetByIdAsync(id, ct);
        if (existingByName is not null && !existingByName.Id.Value.Equals(command.Id))
        {
            var nameExists = await _repository.ExistsByNameAsync(name, ct);
            if (nameExists)
                return Result.Failure($"Provider with name '{command.Name}' already exists.");
        }

        var authorizationEndpoint = OAuthEndpoint.From(command.AuthorizationEndpoint);
        var tokenEndpoint = OAuthEndpoint.From(command.TokenEndpoint);
        var clientId = ClientId.From(command.ClientId);
        var redirectUri = RedirectUri.From(command.RedirectUri);

        ClientSecret? clientSecret = null;
        if (!string.IsNullOrWhiteSpace(command.ClientSecret))
            clientSecret = ClientSecret.From(command.ClientSecret);

        config.Update(
            name,
            authorizationEndpoint,
            tokenEndpoint,
            clientId,
            redirectUri,
            clientSecret,
            ScopeCollection.From(command.DefaultScopes),
            command.SupportedGrantTypes.Select(GrantType.From).ToList());

        config.SetOptionalEndpoints(
            command.RevocationEndpoint is not null ? OAuthEndpoint.From(command.RevocationEndpoint) : null,
            command.IntrospectionEndpoint is not null ? OAuthEndpoint.From(command.IntrospectionEndpoint) : null,
            command.UserInfoEndpoint is not null ? OAuthEndpoint.From(command.UserInfoEndpoint) : null,
            command.Issuer is not null ? OAuthEndpoint.From(command.Issuer) : null);

        await _repository.UpdateAsync(config, ct);

        return Result.Success();
    }
}
