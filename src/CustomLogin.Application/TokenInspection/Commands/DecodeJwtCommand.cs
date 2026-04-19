namespace CustomLogin.Application.TokenInspection.Commands;

public sealed class DecodeJwtCommand
{
    public string Token { get; set; } = string.Empty;
}
