namespace CustomLogin.Contracts.OAuthFlows;

public sealed class FlowSessionResponse
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string FlowType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AuthorizationUrl { get; set; }
    public string? CallbackCode { get; set; }
    public string? CallbackError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
}
