using CustomLogin.Infrastructure.Persistence;
using CustomLogin.Infrastructure.Persistence.Documents;
using CustomLogin.Infrastructure.Persistence.EventSourcing;
using CustomLogin.Infrastructure.Persistence.Repositories;
using CustomLogin.Domain.OAuthFlows;
using FluentAssertions;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace CustomLogin.IntegrationTests.Persistence;

public class FlowSessionRepositoryTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder().Build();
    private IMongoClient _client = null!;
    private MongoDbContext _context = null!;
    private FlowSessionRepository _sessionRepository = null!;
    private MongoEventStore _eventStore = null!;

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
        _client = new MongoClient(_mongoContainer.GetConnectionString());
        _context = new MongoDbContext(_client, "testdb");
        _sessionRepository = new FlowSessionRepository(_context);
        _eventStore = new MongoEventStore(_context);
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.StopAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistSession()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session.GeneratePkceChallenge("verifier", "challenge");
        session.GenerateAuthorizationUrl("http://example.com", "state123");

        await _sessionRepository.AddAsync(session);
        await _eventStore.AppendEventsAsync(session.Id, nameof(OAuthFlowSession), session.UncommittedEvents);
        session.ClearUncommittedEvents();

        var collection = _context.GetCollection<FlowSessionDocument>(MongoCollectionNames.FlowSessions);
        var count = await collection.CountDocumentsAsync(_ => true);
        count.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSession()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session.GeneratePkceChallenge("verifier", "challenge");
        session.GenerateAuthorizationUrl("http://example.com", "state123");
        session.ReceiveCallback("auth-code", "state123", null, null);

        await _sessionRepository.AddAsync(session);

        var result = await _sessionRepository.GetByIdAsync(session.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(session.Id);
        result.Status.Should().Be(FlowSessionStatus.CallbackReceived);
        result.CallbackCode.Should().Be("auth-code");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSession()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session.GeneratePkceChallenge("v", "c");
        session.GenerateAuthorizationUrl("http://example.com", "state");
        await _sessionRepository.AddAsync(session);

        session.ReceiveCallback("code", "state", null, null);
        await _sessionRepository.UpdateAsync(session);

        var result = await _sessionRepository.GetByIdAsync(session.Id);
        result!.Status.Should().Be(FlowSessionStatus.CallbackReceived);
        result.CallbackCode.Should().Be("code");
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAllSessions()
    {
        var session1 = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session1.GeneratePkceChallenge("v1", "c1");
        session1.GenerateAuthorizationUrl("http://example.com", "state1");

        var session2 = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session2.GeneratePkceChallenge("v2", "c2");
        session2.GenerateAuthorizationUrl("http://example.com", "state2");

        await _sessionRepository.AddAsync(session1);
        await _sessionRepository.AddAsync(session2);

        var results = await _sessionRepository.ListAsync();

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task EventStore_AppendAndRetrieve_ShouldWork()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session.GeneratePkceChallenge("verifier", "challenge");
        session.GenerateAuthorizationUrl("http://example.com", "state123");
        session.ReceiveCallback("code", "state123", null, null);

        await _eventStore.AppendEventsAsync(session.Id, nameof(OAuthFlowSession), session.UncommittedEvents);

        var events = await _eventStore.GetEventsAsync(session.Id);

        events.Should().HaveCount(4);
        events[0].Should().BeOfType<OAuthFlowSessionStartedEvent>();
        events[1].Should().BeOfType<PkceChallengeGeneratedEvent>();
        events[2].Should().BeOfType<AuthorizationUrlGeneratedEvent>();
        events[3].Should().BeOfType<OAuthCallbackReceivedEvent>();
    }

    [Fact]
    public async Task EventStore_RebuildSessionFromEvents_ShouldMatchOriginal()
    {
        var session = OAuthFlowSession.Create(Guid.NewGuid(), FlowType.AuthorizationCodePkce);
        session.GeneratePkceChallenge("verifier", "challenge");
        session.GenerateAuthorizationUrl("http://example.com", "state123");
        session.ReceiveCallback("code", "state123", null, null);
        session.MarkTokenExchangeCompleted("Bearer", 3600, "openid", true, true, false);

        await _eventStore.AppendEventsAsync(session.Id, nameof(OAuthFlowSession), session.UncommittedEvents);

        var events = await _eventStore.GetEventsAsync(session.Id);
        var rebuiltSession = OAuthFlowSession.RebuildFromEvents(events);

        rebuiltSession.Id.Should().Be(session.Id);
        rebuiltSession.Status.Should().Be(FlowSessionStatus.TokenExchangeCompleted);
        rebuiltSession.PkceVerifier.Should().Be("verifier");
        rebuiltSession.CallbackCode.Should().Be("code");
    }
}
