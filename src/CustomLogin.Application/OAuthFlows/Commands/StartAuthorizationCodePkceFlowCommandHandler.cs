using System.Security.Cryptography;
using CustomLogin.Application.OAuthFlows;
using CustomLogin.Application.ProviderManagement;
using CustomLogin.Contracts.OAuthFlows;
using CustomLogin.Domain;
using CustomLogin.Domain.OAuthFlows;
using CustomLogin.Domain.ProviderManagement;

namespace CustomLogin.Application.OAuthFlows.Commands;

public sealed class StartAuthorizationCodePkceFlowCommandHandler
{
    private readonly IProviderConfigRepository _providerRepository;
    private readonly IFlowSessionRepository _sessionRepository;
    private readonly IEventStore _eventStore;
    private readonly IPkceService _pkceService;

    public StartAuthorizationCodePkceFlowCommandHandler(
        IProviderConfigRepository providerRepository,
        IFlowSessionRepository sessionRepository,
        IEventStore eventStore,
        IPkceService pkceService)
    {
        _providerRepository = providerRepository;
        _sessionRepository = sessionRepository;
        _eventStore = eventStore;
        _pkceService = pkceService;
    }

    public async Task<Result<StartAuthorizationCodePkceResponse>> Handle(StartAuthorizationCodePkceFlowCommand command, CancellationToken ct = default)
    {
        var provider = await _providerRepository.GetByIdAsync(ProviderId.From(command.ProviderId), ct);
        if (provider is null)
            return Result<StartAuthorizationCodePkceResponse>.Failure("Provider config not found.");

        var session = OAuthFlowSession.Create(command.ProviderId, FlowType.AuthorizationCodePkce);

        var (codeVerifier, codeChallenge) = _pkceService.GeneratePkcePair();
        session.GeneratePkceChallenge(codeVerifier, codeChallenge);

        var state = GenerateCryptographicState();

        var authUrl = BuildAuthorizationUrl(provider, codeChallenge, state);
        session.GenerateAuthorizationUrl(authUrl, state);

        await _sessionRepository.AddAsync(session, ct);
        await _eventStore.AppendEventsAsync(session.Id, nameof(OAuthFlowSession), session.UncommittedEvents, ct);
        session.ClearUncommittedEvents();

        var response = new StartAuthorizationCodePkceResponse
        {
            SessionId = session.Id,
            AuthorizationUrl = authUrl,
            State = state,
            CodeChallenge = codeChallenge,
            CreatedAt = session.CreatedAt
        };

        return Result<StartAuthorizationCodePkceResponse>.Success(response);
    }

    private static string BuildAuthorizationUrl(OAuthProviderConfig provider, string codeChallenge, string state)
    {
        var uriBuilder = new UriBuilder(provider.AuthorizationEndpoint.Value)
        {
            Query = $"response_type=code" +
                    $"&client_id={Uri.EscapeDataString(provider.ClientId.Value)}" +
                    $"&redirect_uri={Uri.EscapeDataString(provider.RedirectUri.Value.ToString())}" +
                    $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                    $"&code_challenge_method=S256" +
                    $"&state={Uri.EscapeDataString(state)}" +
                    (provider.DefaultScopes.Scopes.Count > 0
                        ? $"&scope={Uri.EscapeDataString(string.Join(" ", provider.DefaultScopes.Scopes))}"
                        : string.Empty)
        };

        return uriBuilder.Uri.ToString();
    }

    private static string GenerateCryptographicState()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
