using System.Security.Cryptography;
using CustomLogin.Domain.OAuthFlows;
using FluentAssertions;

namespace CustomLogin.UnitTests.OAuthFlows;

public class PkceTests
{
    [Fact]
    public void CodeVerifier_ShouldBeAtLeast43Characters()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        var verifier = Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        verifier.Length.Should().BeGreaterThanOrEqualTo(43);
    }

    [Fact]
    public void CodeChallenge_ShouldBeDifferentFromVerifier()
    {
        var verifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";
        var challengeBytes = System.Text.Encoding.ASCII.GetBytes(verifier);
        var hash = System.Security.Cryptography.SHA256.HashData(challengeBytes);
        var challenge = Convert.ToBase64String(hash)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        challenge.Should().NotBe(verifier);
        challenge.Length.Should().Be(43);
    }
}
