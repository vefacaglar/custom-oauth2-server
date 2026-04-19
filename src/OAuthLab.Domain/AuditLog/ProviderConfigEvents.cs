using OAuthLab.Domain.ProviderManagement;

namespace OAuthLab.Domain.AuditLog;

public sealed record ProviderConfigCreated(ProviderId ProviderId, string ProviderName, DateTime OccurredAt);
public sealed record ProviderConfigUpdated(ProviderId ProviderId, string ProviderName, DateTime OccurredAt);
public sealed record ProviderConfigDeleted(ProviderId ProviderId, DateTime OccurredAt);
