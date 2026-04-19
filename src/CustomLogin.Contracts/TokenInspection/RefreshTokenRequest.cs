namespace CustomLogin.Contracts.TokenInspection;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
