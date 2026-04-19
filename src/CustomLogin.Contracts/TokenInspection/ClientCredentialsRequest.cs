namespace CustomLogin.Contracts.TokenInspection;

public sealed class ClientCredentialsRequest
{
    public Guid ProviderId { get; set; }
    public string? Scope { get; set; }
}
