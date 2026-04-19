using CustomLogin.Domain.ProviderManagement;
using FluentAssertions;

namespace CustomLogin.UnitTests.ProviderManagement;

public class ProviderConfigTests
{
    [Fact]
    public void Create_ShouldInitializeWithCorrectValues()
    {
        var id = ProviderId.New();
        var name = ProviderName.From("Test Provider");
        var authEndpoint = OAuthEndpoint.From("https://auth.example.com");
        var tokenEndpoint = OAuthEndpoint.From("https://token.example.com");
        var clientId = ClientId.From("client123");
        var redirectUri = RedirectUri.From("https://app.example.com/callback");

        var config = new OAuthProviderConfig(id, name, authEndpoint, tokenEndpoint, clientId, redirectUri);

        config.Id.Should().Be(id);
        config.Name.Should().Be(name);
        config.AuthorizationEndpoint.Should().Be(authEndpoint);
        config.TokenEndpoint.Should().Be(tokenEndpoint);
        config.ClientId.Should().Be(clientId);
        config.RedirectUri.Should().Be(redirectUri);
        config.ClientSecret.Should().BeNull();
        config.DefaultScopes.Scopes.Should().BeEmpty();
    }

    [Fact]
    public void Update_ShouldUpdateAllFields()
    {
        var config = CreateTestConfig();
        var newName = ProviderName.From("Updated Provider");
        var newAuth = OAuthEndpoint.From("https://new-auth.example.com");
        var newToken = OAuthEndpoint.From("https://new-token.example.com");
        var newClientId = ClientId.From("new-client");
        var newRedirect = RedirectUri.From("https://new-redirect.example.com");
        var newSecret = ClientSecret.From("new-secret");
        var newScopes = ScopeCollection.From(["openid", "profile"]);
        var newGrants = new List<GrantType> { GrantType.AuthorizationCode };

        config.Update(newName, newAuth, newToken, newClientId, newRedirect, newSecret, newScopes, newGrants);

        config.Name.Should().Be(newName);
        config.AuthorizationEndpoint.Should().Be(newAuth);
        config.TokenEndpoint.Should().Be(newToken);
        config.ClientId.Should().Be(newClientId);
        config.RedirectUri.Should().Be(newRedirect);
        config.ClientSecret.Should().Be(newSecret);
        config.DefaultScopes.Scopes.Should().ContainInOrder("openid", "profile");
        config.SupportedGrantTypes.Should().HaveCount(1);
    }

    [Fact]
    public void ProviderName_ShouldThrow_WhenEmpty()
    {
        Action act = () => ProviderName.From("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OAuthEndpoint_ShouldThrow_WhenInvalidUri()
    {
        Action act = () => OAuthEndpoint.From("not-a-valid-uri");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ClientId_ShouldThrow_WhenEmpty()
    {
        Action act = () => ClientId.From("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RedirectUri_ShouldThrow_WhenInvalidUri()
    {
        Action act = () => RedirectUri.From("not-a-valid-uri");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ScopeCollection_ShouldNormalizeAndDeduplicate()
    {
        var scopes = ScopeCollection.From(new[] { "openid", "profile", "OPENID", "  ", "email" });

        scopes.Scopes.Should().HaveCount(3);
        scopes.Scopes.Should().Contain("openid");
        scopes.Scopes.Should().Contain("profile");
        scopes.Scopes.Should().Contain("email");
    }

    [Fact]
    public void SetOptionalEndpoints_ShouldSetEndpoints()
    {
        var config = CreateTestConfig();
        var revocation = OAuthEndpoint.From("https://revoke.example.com");
        var introspection = OAuthEndpoint.From("https://introspect.example.com");
        var userInfo = OAuthEndpoint.From("https://userinfo.example.com");
        var issuer = OAuthEndpoint.From("https://issuer.example.com");

        config.SetOptionalEndpoints(revocation, introspection, userInfo, issuer);

        config.RevocationEndpoint.Should().Be(revocation);
        config.IntrospectionEndpoint.Should().Be(introspection);
        config.UserInfoEndpoint.Should().Be(userInfo);
        config.Issuer.Should().Be(issuer);
    }

    private static OAuthProviderConfig CreateTestConfig()
    {
        return new OAuthProviderConfig(
            ProviderId.New(),
            ProviderName.From("Test"),
            OAuthEndpoint.From("https://auth.example.com"),
            OAuthEndpoint.From("https://token.example.com"),
            ClientId.From("client123"),
            RedirectUri.From("https://app.example.com/callback"));
    }
}
