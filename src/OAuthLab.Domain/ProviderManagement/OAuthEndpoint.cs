namespace OAuthLab.Domain.ProviderManagement;

public sealed class OAuthEndpoint
{
    public Uri Value { get; }

    private OAuthEndpoint(Uri value)
    {
        if (!value.IsAbsoluteUri)
            throw new ArgumentException("Endpoint must be an absolute URI.", nameof(value));

        Value = value;
    }

    public static OAuthEndpoint From(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalid endpoint URL.", nameof(value));

        return new OAuthEndpoint(uri);
    }
}
