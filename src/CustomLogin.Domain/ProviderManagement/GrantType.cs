namespace CustomLogin.Domain.ProviderManagement;

public sealed class GrantType
{
    public string Value { get; }

    private GrantType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Grant type cannot be empty.", nameof(value));

        Value = value;
    }

    public static GrantType AuthorizationCode => new("authorization_code");
    public static GrantType ClientCredentials => new("client_credentials");
    public static GrantType RefreshToken => new("refresh_token");
    public static GrantType DeviceCode => new("urn:ietf:params:oauth:grant-type:device_code");

    public static GrantType From(string value) => new(value);
}
