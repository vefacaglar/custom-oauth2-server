namespace OAuthLab.Domain.ProviderManagement;

public sealed class ScopeCollection
{
    public IReadOnlyList<string> Scopes { get; }

    private ScopeCollection(IReadOnlyList<string> scopes)
    {
        Scopes = scopes;
    }

    public static ScopeCollection From(IEnumerable<string> scopes)
    {
        var normalized = scopes
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new ScopeCollection(normalized);
    }

    public static ScopeCollection Empty() => new([]);
}
