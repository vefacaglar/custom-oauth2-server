namespace CustomLogin.Domain.TokenInspection;

public sealed class DecodedJwt
{
    public string Algorithm { get; }
    public string? KeyId { get; }
    public string? Issuer { get; }
    public string? Audience { get; }
    public string? Subject { get; }
    public DateTime? Expiration { get; }
    public DateTime? IssuedAt { get; }
    public DateTime? NotBefore { get; }
    public IReadOnlyList<string> Scopes { get; }
    public IReadOnlyDictionary<string, object> Claims { get; }
    public string RawHeader { get; }
    public string RawPayload { get; }
    public bool IsValidFormat { get; }
    public string? FormatError { get; }

    private DecodedJwt(
        string algorithm,
        string? keyId,
        string? issuer,
        string? audience,
        string? subject,
        DateTime? expiration,
        DateTime? issuedAt,
        DateTime? notBefore,
        IReadOnlyList<string> scopes,
        IReadOnlyDictionary<string, object> claims,
        string rawHeader,
        string rawPayload,
        bool isValidFormat,
        string? formatError)
    {
        Algorithm = algorithm;
        KeyId = keyId;
        Issuer = issuer;
        Audience = audience;
        Subject = subject;
        Expiration = expiration;
        IssuedAt = issuedAt;
        NotBefore = notBefore;
        Scopes = scopes;
        Claims = claims;
        RawHeader = rawHeader;
        RawPayload = rawPayload;
        IsValidFormat = isValidFormat;
        FormatError = formatError;
    }

    public static DecodedJwt Decode(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return new DecodedJwt(
                algorithm: string.Empty,
                keyId: null,
                issuer: null,
                audience: null,
                subject: null,
                expiration: null,
                issuedAt: null,
                notBefore: null,
                scopes: [],
                claims: new Dictionary<string, object>(),
                rawHeader: string.Empty,
                rawPayload: string.Empty,
                isValidFormat: false,
                formatError: "JWT must contain exactly 3 parts separated by dots.");
        }

        string headerJson, payloadJson;
        try
        {
            headerJson = DecodeBase64Url(parts[0]);
            payloadJson = DecodeBase64Url(parts[1]);
        }
        catch (Exception ex)
        {
            return new DecodedJwt(
                algorithm: string.Empty,
                keyId: null,
                issuer: null,
                audience: null,
                subject: null,
                expiration: null,
                issuedAt: null,
                notBefore: null,
                scopes: [],
                claims: new Dictionary<string, object>(),
                rawHeader: string.Empty,
                rawPayload: string.Empty,
                isValidFormat: false,
                formatError: $"Failed to decode Base64URL: {ex.Message}");
        }

        Dictionary<string, System.Text.Json.JsonElement>? header = null;
        Dictionary<string, System.Text.Json.JsonElement>? payload = null;
        try
        {
            header = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(headerJson);
            payload = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(payloadJson);
        }
        catch (System.Text.Json.JsonException ex)
        {
            return new DecodedJwt(
                algorithm: string.Empty,
                keyId: null,
                issuer: null,
                audience: null,
                subject: null,
                expiration: null,
                issuedAt: null,
                notBefore: null,
                scopes: [],
                claims: new Dictionary<string, object>(),
                rawHeader: headerJson,
                rawPayload: payloadJson,
                isValidFormat: false,
                formatError: $"Failed to parse JWT header or payload as JSON: {ex.Message}");
        }

        if (header is null || payload is null)
        {
            return new DecodedJwt(
                algorithm: string.Empty,
                keyId: null,
                issuer: null,
                audience: null,
                subject: null,
                expiration: null,
                issuedAt: null,
                notBefore: null,
                scopes: [],
                claims: new Dictionary<string, object>(),
                rawHeader: headerJson,
                rawPayload: payloadJson,
                isValidFormat: false,
                formatError: "Failed to parse JWT header or payload as JSON.");
        }

        var algorithm = header.TryGetValue("alg", out var algEl) ? algEl.GetString() ?? string.Empty : string.Empty;
        var keyId = header.TryGetValue("kid", out var kidEl) ? kidEl.GetString() : null;

        var issuer = payload.TryGetValue("iss", out var issEl) ? issEl.GetString() : null;
        var audience = payload.TryGetValue("aud", out var audEl) ? audEl.ValueKind == System.Text.Json.JsonValueKind.String ? audEl.GetString() : audEl.GetRawText() : null;
        var subject = payload.TryGetValue("sub", out var subEl) ? subEl.GetString() : null;

        DateTime? expiration = null;
        if (payload.TryGetValue("exp", out var expEl) && expEl.ValueKind == System.Text.Json.JsonValueKind.Number)
            expiration = DateTimeOffset.FromUnixTimeSeconds(expEl.GetInt64()).UtcDateTime;

        DateTime? issuedAt = null;
        if (payload.TryGetValue("iat", out var iatEl) && iatEl.ValueKind == System.Text.Json.JsonValueKind.Number)
            issuedAt = DateTimeOffset.FromUnixTimeSeconds(iatEl.GetInt64()).UtcDateTime;

        DateTime? notBefore = null;
        if (payload.TryGetValue("nbf", out var nbfEl) && nbfEl.ValueKind == System.Text.Json.JsonValueKind.Number)
            notBefore = DateTimeOffset.FromUnixTimeSeconds(nbfEl.GetInt64()).UtcDateTime;

        var scopes = new List<string>();
        if (payload.TryGetValue("scope", out var scopeEl) && scopeEl.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            var scopeStr = scopeEl.GetString();
            if (!string.IsNullOrEmpty(scopeStr))
                scopes = scopeStr.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        var claims = new Dictionary<string, object>();
        foreach (var kvp in payload)
        {
            claims[kvp.Key] = kvp.Value.ValueKind switch
            {
                System.Text.Json.JsonValueKind.String => kvp.Value.GetString()!,
                System.Text.Json.JsonValueKind.Number => kvp.Value.GetDouble(),
                System.Text.Json.JsonValueKind.True => true,
                System.Text.Json.JsonValueKind.False => false,
                System.Text.Json.JsonValueKind.Null => "null",
                _ => kvp.Value.GetRawText()
            };
        }

        return new DecodedJwt(
            algorithm: algorithm,
            keyId: keyId,
            issuer: issuer,
            audience: audience,
            subject: subject,
            expiration: expiration,
            issuedAt: issuedAt,
            notBefore: notBefore,
            scopes: scopes,
            claims: claims,
            rawHeader: headerJson,
            rawPayload: payloadJson,
            isValidFormat: true,
            formatError: null);
    }

    private static string DecodeBase64Url(string input)
    {
        var padded = input.Length % 4 == 0
            ? input
            : input.PadRight(input.Length + (4 - input.Length % 4), '=');

        var base64 = padded.Replace('-', '+').Replace('_', '/');
        var bytes = Convert.FromBase64String(base64);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
