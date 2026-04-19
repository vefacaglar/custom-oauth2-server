namespace CustomLogin.Domain.ProviderManagement;

public sealed class ClientId
{
    public string Value { get; }

    private ClientId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Client ID cannot be empty.", nameof(value));

        Value = value;
    }

    public static ClientId From(string value) => new(value);
}
