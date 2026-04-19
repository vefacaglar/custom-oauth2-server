using CustomLogin.Contracts.TokenInspection;
using CustomLogin.Domain;
using CustomLogin.Domain.TokenInspection;

namespace CustomLogin.Application.TokenInspection.Commands;

public sealed class DecodeJwtCommandHandler
{
    public Result<DecodedJwtResponse> Handle(DecodeJwtCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            return Result<DecodedJwtResponse>.Failure("Token is required.");

        var decoded = DecodedJwt.Decode(command.Token);

        if (!decoded.IsValidFormat)
            return Result<DecodedJwtResponse>.Failure(decoded.FormatError ?? "Invalid JWT format.");

        var response = new DecodedJwtResponse
        {
            Algorithm = decoded.Algorithm,
            KeyId = decoded.KeyId,
            Issuer = decoded.Issuer,
            Audience = decoded.Audience,
            Subject = decoded.Subject,
            Expiration = decoded.Expiration,
            IssuedAt = decoded.IssuedAt,
            NotBefore = decoded.NotBefore,
            Scopes = decoded.Scopes,
            Claims = decoded.Claims,
            RawHeader = decoded.RawHeader,
            RawPayload = decoded.RawPayload,
            IsValidFormat = decoded.IsValidFormat,
            FormatError = decoded.FormatError,
            IsExpired = decoded.Expiration.HasValue && decoded.Expiration.Value < DateTime.UtcNow
        };

        return Result<DecodedJwtResponse>.Success(response);
    }
}
