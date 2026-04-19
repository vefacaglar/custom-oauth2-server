namespace CustomLogin.Contracts.TokenInspection;

public sealed class DecodeJwtRequest
{
    public string Token { get; set; } = string.Empty;
}
