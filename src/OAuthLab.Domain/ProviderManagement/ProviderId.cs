namespace OAuthLab.Domain.ProviderManagement;

public sealed class ProviderId
{
    public Guid Value { get; }

    private ProviderId(Guid value)
    {
        Value = value;
    }

    public static ProviderId New() => new(Guid.NewGuid());
    public static ProviderId From(Guid value) => new(value);
}
