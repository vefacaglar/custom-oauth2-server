using CustomLogin.Domain.TokenInspection;
using FluentAssertions;

namespace CustomLogin.UnitTests.TokenInspection;

public class DecodedJwtTests
{
    private const string ValidJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6InRlc3Qta2V5In0.eyJpc3MiOiJodHRwczovL2V4YW1wbGUuY29tIiwiYXVkIjoidGVzdC1hcHAiLCJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiZXhwIjoxOTAwMDAwMDAwLCJpYXQiOjE1MTYyMzkwMjIsInNjb3BlIjoib3BlbmlkIHByb2ZpbGUgZW1haWwiLCJyb2xlIjoiYWRtaW4ifQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

    [Fact]
    public void Decode_ShouldParseValidJwt()
    {
        var decoded = DecodedJwt.Decode(ValidJwt);

        decoded.IsValidFormat.Should().BeTrue();
        decoded.Algorithm.Should().Be("HS256");
        decoded.KeyId.Should().Be("test-key");
        decoded.Issuer.Should().Be("https://example.com");
        decoded.Audience.Should().Be("test-app");
        decoded.Subject.Should().Be("1234567890");
        decoded.Expiration.Should().Be(new DateTime(2030, 3, 17, 17, 46, 40, DateTimeKind.Utc));
        decoded.IssuedAt.Should().Be(new DateTime(2018, 1, 18, 1, 30, 22, DateTimeKind.Utc));
        decoded.Scopes.Should().ContainInOrder("openid", "profile", "email");
        decoded.Claims.Should().ContainKey("name").WhoseValue.Should().Be("John Doe");
        decoded.Claims.Should().ContainKey("role").WhoseValue.Should().Be("admin");
    }

    [Fact]
    public void Decode_ShouldReturnInvalid_WhenNotThreeParts()
    {
        var decoded = DecodedJwt.Decode("not.a.jwt.extra");

        decoded.IsValidFormat.Should().BeFalse();
        decoded.FormatError.Should().Contain("3 parts");
    }

    [Fact]
    public void Decode_ShouldReturnInvalid_WhenOnlyTwoParts()
    {
        var decoded = DecodedJwt.Decode("header.payload");

        decoded.IsValidFormat.Should().BeFalse();
    }

    [Fact]
    public void Decode_ShouldReturnInvalid_WhenInvalidBase64()
    {
        var decoded = DecodedJwt.Decode("!!!.!!!.!!!");

        decoded.IsValidFormat.Should().BeFalse();
        decoded.FormatError.Should().Contain("Base64URL");
    }

    [Fact]
    public void Decode_ShouldReturnInvalid_WhenInvalidJson()
    {
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("not-json"));
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"valid\": true}"));
        var token = $"{header}.{payload}.signature";

        var decoded = DecodedJwt.Decode(token);

        decoded.IsValidFormat.Should().BeFalse();
    }

    [Fact]
    public void Decode_ShouldHandleMissingOptionalClaims()
    {
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"RS256\"}"));
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"sub\":\"user1\"}"));
        var token = $"{header}.{payload}.sig";

        var decoded = DecodedJwt.Decode(token);

        decoded.IsValidFormat.Should().BeTrue();
        decoded.Algorithm.Should().Be("RS256");
        decoded.Issuer.Should().BeNull();
        decoded.Audience.Should().BeNull();
        decoded.Expiration.Should().BeNull();
        decoded.Scopes.Should().BeEmpty();
    }

    [Fact]
    public void Decode_ShouldParseAudienceAsArray()
    {
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"RS256\"}"));
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"aud\":[\"app1\",\"app2\"]}"));
        var token = $"{header}.{payload}.sig";

        var decoded = DecodedJwt.Decode(token);

        decoded.IsValidFormat.Should().BeTrue();
        decoded.Audience.Should().Be("[\"app1\",\"app2\"]");
    }

    [Fact]
    public void Decode_ShouldDetectExpiredToken()
    {
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"HS256\"}"));
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"exp\":1000000000}"));
        var token = $"{header}.{payload}.sig";

        var decoded = DecodedJwt.Decode(token);

        decoded.Expiration.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1000000000).UtcDateTime);
    }
}
