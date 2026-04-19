namespace CustomLogin.Domain.TokenInspection;

public sealed class TokenResponseId
{
    public Guid Value { get; }

    private TokenResponseId(Guid value)
    {
        Value = value;
    }

    public static TokenResponseId New() => new(Guid.NewGuid());
    public static TokenResponseId From(Guid value) => new(value);
}
