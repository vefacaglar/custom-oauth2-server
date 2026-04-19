namespace OAuthLab.Infrastructure.Persistence;

public static class MongoCollectionNames
{
    public const string ProviderConfigs = "provider_configs";
    public const string FlowSessions = "oauth_flow_sessions";
    public const string TokenResponses = "token_responses";
    public const string EventStore = "event_store";
    public const string AuditEvents = "audit_events";
}
