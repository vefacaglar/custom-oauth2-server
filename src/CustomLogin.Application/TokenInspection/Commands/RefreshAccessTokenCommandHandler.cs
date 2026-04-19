using CustomLogin.Application.OAuthFlows;
using CustomLogin.Application.ProviderManagement;
using CustomLogin.Application.TokenInspection;
using CustomLogin.Contracts.TokenInspection;
using CustomLogin.Domain;
using CustomLogin.Domain.OAuthFlows;
using CustomLogin.Domain.ProviderManagement;
using CustomLogin.Domain.TokenInspection;

namespace CustomLogin.Application.TokenInspection.Commands;

public sealed class RefreshAccessTokenCommandHandler
{
    private readonly IFlowSessionRepository _sessionRepository;
    private readonly IProviderConfigRepository _providerRepository;
    private readonly ITokenEndpointClient _tokenClient;
    private readonly ITokenResponseRepository _tokenResponseRepository;
    private readonly IEventStore _eventStore;

    public RefreshAccessTokenCommandHandler(
        IFlowSessionRepository sessionRepository,
        IProviderConfigRepository providerRepository,
        ITokenEndpointClient tokenClient,
        ITokenResponseRepository tokenResponseRepository,
        IEventStore eventStore)
    {
        _sessionRepository = sessionRepository;
        _providerRepository = providerRepository;
        _tokenClient = tokenClient;
        _tokenResponseRepository = tokenResponseRepository;
        _eventStore = eventStore;
    }

    public async Task<Result<TokenExchangeResponse>> Handle(RefreshAccessTokenCommand command, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(command.FlowSessionId, ct);
        if (session is null)
            return Result<TokenExchangeResponse>.Failure("Flow session not found.");

        var provider = await _providerRepository.GetByIdAsync(ProviderId.From(session.ProviderId), ct);
        if (provider is null)
            return Result<TokenExchangeResponse>.Failure("Provider config not found.");

        TokenEndpointResponse tokenResult;
        try
        {
            tokenResult = await _tokenClient.RefreshAccessTokenAsync(
                provider.TokenEndpoint.Value.ToString(),
                provider.ClientId.Value,
                provider.ClientSecret?.Value,
                command.RefreshToken,
                ct);
        }
        catch (Exception ex)
        {
            return Result<TokenExchangeResponse>.Failure($"Refresh token request failed: {ex.Message}");
        }

        if (!string.IsNullOrEmpty(tokenResult.Error))
            return Result<TokenExchangeResponse>.Failure($"Refresh token error: {tokenResult.ErrorDescription ?? tokenResult.Error}");

        if (string.IsNullOrEmpty(tokenResult.AccessToken))
            return Result<TokenExchangeResponse>.Failure("Refresh token returned no access token.");

        var tokenRecord = new TokenResponseRecord(
            TokenResponseId.New(),
            session.Id,
            session.ProviderId,
            tokenResult.AccessToken,
            tokenResult.RefreshToken,
            tokenResult.IdToken,
            tokenResult.TokenType,
            tokenResult.ExpiresIn,
            tokenResult.Scope,
            tokenResult.RawResponse);

        await _tokenResponseRepository.AddAsync(tokenRecord, ct);

        var response = BuildResponse(tokenRecord);
        return Result<TokenExchangeResponse>.Success(response);
    }

    private static TokenExchangeResponse BuildResponse(TokenResponseRecord record)
    {
        var response = new TokenExchangeResponse
        {
            TokenResponseId = record.Id.Value,
            FlowSessionId = record.FlowSessionId,
            TokenType = record.TokenType,
            ExpiresIn = record.ExpiresIn,
            Scope = record.Scope,
            MaskedAccessToken = record.MaskAccessToken(),
            MaskedRefreshToken = record.MaskRefreshToken(),
            MaskedIdToken = record.MaskIdToken(),
            CreatedAt = record.CreatedAt
        };

        if (!string.IsNullOrEmpty(record.AccessToken))
        {
            var decoded = DecodedJwt.Decode(record.AccessToken);
            response.DecodedAccessToken = MapDecodedJwt(decoded);
        }

        if (!string.IsNullOrEmpty(record.IdToken))
        {
            var decoded = DecodedJwt.Decode(record.IdToken);
            response.DecodedIdToken = MapDecodedJwt(decoded);
        }

        return response;
    }

    private static DecodedJwtResponse MapDecodedJwt(DecodedJwt jwt)
    {
        return new DecodedJwtResponse
        {
            Algorithm = jwt.Algorithm,
            KeyId = jwt.KeyId,
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Subject = jwt.Subject,
            Expiration = jwt.Expiration,
            IssuedAt = jwt.IssuedAt,
            NotBefore = jwt.NotBefore,
            Scopes = jwt.Scopes,
            Claims = jwt.Claims,
            RawHeader = jwt.RawHeader,
            RawPayload = jwt.RawPayload,
            IsValidFormat = jwt.IsValidFormat,
            FormatError = jwt.FormatError,
            IsExpired = jwt.Expiration.HasValue && jwt.Expiration.Value < DateTime.UtcNow
        };
    }
}
