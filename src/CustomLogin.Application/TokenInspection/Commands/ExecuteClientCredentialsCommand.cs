namespace CustomLogin.Application.TokenInspection.Commands;

public sealed class ExecuteClientCredentialsCommand
{
    public Guid ProviderId { get; set; }
    public string? Scope { get; set; }
}
