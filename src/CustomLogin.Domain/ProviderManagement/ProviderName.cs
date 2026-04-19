namespace CustomLogin.Domain.ProviderManagement;

public sealed class ProviderName
{
    public string Value { get; }

    private ProviderName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Provider name cannot be empty.", nameof(value));

        Value = value.Trim();
    }

    public static ProviderName From(string value) => new(value);
}
