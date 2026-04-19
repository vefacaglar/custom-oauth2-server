namespace OAuthLab.Domain.ProviderManagement;

public sealed class ClientSecret
{
    public string Value { get; }

    private ClientSecret(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Client secret cannot be empty.", nameof(value));

        Value = value;
    }

    public static ClientSecret From(string value) => new(value);
}
