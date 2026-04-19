namespace CustomLogin.Domain.ProviderManagement;

public sealed class RedirectUri
{
    public Uri Value { get; }

    private RedirectUri(Uri value)
    {
        if (!value.IsAbsoluteUri)
            throw new ArgumentException("Redirect URI must be an absolute URI.", nameof(value));

        Value = value;
    }

    public static RedirectUri From(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalid redirect URI.", nameof(value));

        return new RedirectUri(uri);
    }
}
