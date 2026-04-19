namespace CustomLogin.Domain.OAuthFlows;

public enum FlowSessionStatus
{
    Created,
    AuthorizationUrlGenerated,
    CallbackReceived,
    TokenExchangeCompleted,
    Failed,
    Expired
}
