namespace CustomLogin.Application.TokenInspection.Commands;

public sealed class RefreshAccessTokenCommand
{
    public Guid FlowSessionId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}
