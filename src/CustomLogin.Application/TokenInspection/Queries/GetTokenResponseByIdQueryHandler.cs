using CustomLogin.Application.Dispatcher;

using CustomLogin.Contracts.TokenInspection;
using CustomLogin.Domain;
using CustomLogin.Domain.TokenInspection;

namespace CustomLogin.Application.TokenInspection.Queries;

public sealed class GetTokenResponseByIdQueryHandler : IQueryHandler<GetTokenResponseByIdQuery, TokenResponseSummary>
{
    private readonly ITokenResponseRepository _repository;

    public GetTokenResponseByIdQueryHandler(ITokenResponseRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TokenResponseSummary>> Handle(GetTokenResponseByIdQuery query, CancellationToken ct = default)
    {
        var record = await _repository.GetByIdAsync(TokenResponseId.From(query.Id), ct);
        if (record is null)
            return Result<TokenResponseSummary>.Failure("Token response not found.");

        var response = MapToSummary(record);
        return Result<TokenResponseSummary>.Success(response);
    }

    private static TokenResponseSummary MapToSummary(TokenResponseRecord record)
    {
        return new TokenResponseSummary
        {
            Id = record.Id.Value,
            FlowSessionId = record.FlowSessionId,
            ProviderId = record.ProviderId,
            TokenType = record.TokenType,
            ExpiresIn = record.ExpiresIn,
            Scope = record.Scope,
            MaskedAccessToken = record.MaskAccessToken(),
            MaskedRefreshToken = record.MaskRefreshToken(),
            MaskedIdToken = record.MaskIdToken(),
            HasAccessToken = !string.IsNullOrEmpty(record.AccessToken),
            HasRefreshToken = !string.IsNullOrEmpty(record.RefreshToken),
            HasIdToken = !string.IsNullOrEmpty(record.IdToken),
            CreatedAt = record.CreatedAt
        };
    }
}
