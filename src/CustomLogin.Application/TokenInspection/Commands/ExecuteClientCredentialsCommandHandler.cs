using CustomLogin.Application.Dispatcher;

using CustomLogin.Application.ProviderManagement;
using CustomLogin.Application.TokenInspection;
using CustomLogin.Contracts.TokenInspection;
using CustomLogin.Domain;
using CustomLogin.Domain.ProviderManagement;
using CustomLogin.Domain.TokenInspection;

namespace CustomLogin.Application.TokenInspection.Commands;

public sealed class ExecuteClientCredentialsCommandHandler : ICommandHandler<ExecuteClientCredentialsCommand, TokenExchangeResponse>
{
    private readonly IProviderConfigRepository _providerRepository;
    private readonly ITokenEndpointClient _tokenClient;
    private readonly ITokenResponseRepository _tokenResponseRepository;

    public ExecuteClientCredentialsCommandHandler(
        IProviderConfigRepository providerRepository,
        ITokenEndpointClient tokenClient,
        ITokenResponseRepository tokenResponseRepository)
    {
        _providerRepository = providerRepository;
        _tokenClient = tokenClient;
        _tokenResponseRepository = tokenResponseRepository;
    }

    public async Task<Result<TokenExchangeResponse>> Handle(ExecuteClientCredentialsCommand command, CancellationToken ct = default)
    {
        var provider = await _providerRepository.GetByIdAsync(ProviderId.From(command.ProviderId), ct);
        if (provider is null)
            return Result<TokenExchangeResponse>.Failure("Provider config not found.");

        if (string.IsNullOrEmpty(provider.ClientSecret?.Value))
            return Result<TokenExchangeResponse>.Failure("Client secret is required for client credentials flow.");

        TokenEndpointResponse tokenResult;
        try
        {
            tokenResult = await _tokenClient.ExecuteClientCredentialsAsync(
                provider.TokenEndpoint.Value.ToString(),
                provider.ClientId.Value,
                provider.ClientSecret.Value,
                command.Scope ?? (provider.DefaultScopes.Scopes.Count > 0
                    ? string.Join(" ", provider.DefaultScopes.Scopes)
                    : null),
                ct);
        }
        catch (Exception ex)
        {
            return Result<TokenExchangeResponse>.Failure($"Token request failed: {ex.Message}");
        }

        if (!string.IsNullOrEmpty(tokenResult.Error))
            return Result<TokenExchangeResponse>.Failure($"Token error: {tokenResult.ErrorDescription ?? tokenResult.Error}");

        if (string.IsNullOrEmpty(tokenResult.AccessToken))
            return Result<TokenExchangeResponse>.Failure("Token exchange returned no access token.");

        var tokenRecord = new TokenResponseRecord(
            TokenResponseId.New(),
            flowSessionId: Guid.Empty,
            provider.Id.Value,
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
