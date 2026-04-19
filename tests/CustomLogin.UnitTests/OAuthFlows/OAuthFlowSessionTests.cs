using CustomLogin.Domain.OAuthFlows;
using FluentAssertions;

namespace CustomLogin.UnitTests.OAuthFlows;

public class OAuthFlowSessionTests
{
    [Fact]
    public void Create_ShouldInitializeWithCorrectState()
    {
        var providerId = Guid.NewGuid();
        var session = OAuthFlowSession.Create(providerId, FlowType.AuthorizationCodePkce);

        session.Id.Should().NotBeEmpty();
        session.ProviderId.Should().Be(providerId);
        session.FlowType.Should().Be(FlowType.AuthorizationCodePkce);
        session.Status.Should().Be(FlowSessionStatus.Created);
        session.UncommittedEvents.Should().HaveCount(1);
        session.UncommittedEvents.First().Should().BeOfType<OAuthFlowSessionStartedEvent>();
    }

    [Fact]
    public void GeneratePkceChallenge_ShouldGenerateChallengeAndEvent()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);

        session.GeneratePkceChallenge("verifier123", "challenge456");

        session.PkceVerifier.Should().Be("verifier123");
        session.PkceChallenge.Should().Be("challenge456");
        session.Status.Should().Be(FlowSessionStatus.Created);
        session.UncommittedEvents.Should().HaveCount(2);
        session.UncommittedEvents.Last().Should().BeOfType<PkceChallengeGeneratedEvent>();
    }

    [Fact]
    public void GeneratePkceChallenge_ShouldThrow_IfNotInCreatedState()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session.GeneratePkceChallenge("v", "c");
        session.GenerateAuthorizationUrl("http://example.com", "state123");

        Action act = () => session.GeneratePkceChallenge("v2", "c2");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("PKCE can only be generated in Created state.");
    }

    [Fact]
    public void GenerateAuthorizationUrl_ShouldThrow_IfNoPkceVerifier()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);

        Action act = () => session.GenerateAuthorizationUrl("http://example.com", "state123");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("PKCE verifier must be generated before authorization URL.");
    }

    [Fact]
    public void GenerateAuthorizationUrl_ShouldUpdateStateAndStatus()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session.GeneratePkceChallenge("verifier", "challenge");

        session.GenerateAuthorizationUrl("http://example.com/auth", "state123");

        session.AuthorizationUrl.Should().Be("http://example.com/auth");
        session.State.Should().Be("state123");
        session.Status.Should().Be(FlowSessionStatus.AuthorizationUrlGenerated);
        session.UncommittedEvents.Last().Should().BeOfType<AuthorizationUrlGeneratedEvent>();
    }

    [Fact]
    public void ReceiveCallback_ShouldStoreCode_WhenSuccessful()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session.GeneratePkceChallenge("v", "c");
        session.GenerateAuthorizationUrl("http://example.com", "state123");

        session.ReceiveCallback("auth-code-123", "state123", null, null);

        session.CallbackCode.Should().Be("auth-code-123");
        session.Status.Should().Be(FlowSessionStatus.CallbackReceived);
        session.UncommittedEvents.Last().Should().BeOfType<OAuthCallbackReceivedEvent>();
    }

    [Fact]
    public void ReceiveCallback_ShouldFail_WhenStateMismatch()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session.GeneratePkceChallenge("v", "c");
        session.GenerateAuthorizationUrl("http://example.com", "state123");

        Action act = () => session.ReceiveCallback("code", "wrong-state", null, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("State mismatch. Expected callback state to match stored state.");
    }

    [Fact]
    public void ReceiveCallback_ShouldMarkFailed_WhenError()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session.GeneratePkceChallenge("v", "c");
        session.GenerateAuthorizationUrl("http://example.com", "state123");

        session.ReceiveCallback(null, "state123", "access_denied", "User denied access");

        session.CallbackError.Should().Be("access_denied");
        session.Status.Should().Be(FlowSessionStatus.Failed);
        session.FailedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkTokenExchangeCompleted_ShouldUpdateStatus()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session.GeneratePkceChallenge("v", "c");
        session.GenerateAuthorizationUrl("http://example.com", "state");
        session.ReceiveCallback("code", "state", null, null);

        session.MarkTokenExchangeCompleted("Bearer", 3600, "openid", true, true, false);

        session.Status.Should().Be(FlowSessionStatus.TokenExchangeCompleted);
        session.CompletedAt.Should().NotBeNull();
        session.UncommittedEvents.Last().Should().BeOfType<AuthorizationCodeExchangedForTokenEvent>();
    }

    [Fact]
    public void MarkExpired_ShouldThrow_IfCompleted()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session.GeneratePkceChallenge("v", "c");
        session.GenerateAuthorizationUrl("http://example.com", "state");
        session.ReceiveCallback("code", "state", null, null);
        session.MarkTokenExchangeCompleted("Bearer", 3600, "openid", true, false, false);

        Action act = () => session.MarkExpired();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RebuildFromEvents_ShouldReconstructSession()
    {
        var sessionId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var events = new List<IDomainEvent>
        {
            new OAuthFlowSessionStartedEvent(sessionId, providerId, FlowType.AuthorizationCodePkce, DateTime.UtcNow),
            new PkceChallengeGeneratedEvent(sessionId, "challenge", "verifier", DateTime.UtcNow),
            new AuthorizationUrlGeneratedEvent(sessionId, "http://example.com", "state123", DateTime.UtcNow),
            new OAuthCallbackReceivedEvent(sessionId, "auth-code", "state123", null, null, DateTime.UtcNow)
        };

        var session = OAuthFlowSession.RebuildFromEvents(events);

        session.Id.Should().Be(sessionId);
        session.ProviderId.Should().Be(providerId);
        session.FlowType.Should().Be(FlowType.AuthorizationCodePkce);
        session.Status.Should().Be(FlowSessionStatus.CallbackReceived);
        session.PkceVerifier.Should().Be("verifier");
        session.PkceChallenge.Should().Be("challenge");
        session.AuthorizationUrl.Should().Be("http://example.com");
        session.State.Should().Be("state123");
        session.CallbackCode.Should().Be("auth-code");
    }
}
