using CustomLogin.Infrastructure.Persistence;
using CustomLogin.Infrastructure.Persistence.Documents;
using CustomLogin.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace CustomLogin.IntegrationTests.Persistence;

public class ProviderConfigRepositoryTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder().Build();
    private IMongoClient _client = null!;
    private MongoDbContext _context = null!;
    private ProviderConfigRepository _repository = null!;

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
        _client = new MongoClient(_mongoContainer.GetConnectionString());
        _context = new MongoDbContext(_client, "testdb");
        _repository = new ProviderConfigRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.StopAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistProviderConfig()
    {
        var config = CreateTestConfig();
        await _repository.AddAsync(config);

        var collection = _context.GetCollection<ProviderConfigDocument>(MongoCollectionNames.ProviderConfigs);
        var count = await collection.CountDocumentsAsync(_ => true);
        count.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnConfig()
    {
        var config = CreateTestConfig();
        await _repository.AddAsync(config);

        var result = await _repository.GetByIdAsync(config.Id);

        result.Should().NotBeNull();
        result!.Id.Value.Should().Be(config.Id.Value);
        result.Name.Value.Should().Be(config.Name.Value);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(Domain.ProviderManagement.ProviderId.New());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue_WhenExists()
    {
        var config = CreateTestConfig();
        await _repository.AddAsync(config);

        var exists = await _repository.ExistsByNameAsync(config.Name);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse_WhenNotExists()
    {
        var exists = await _repository.ExistsByNameAsync(Domain.ProviderManagement.ProviderName.From("NonExistent"));

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateConfig()
    {
        var config = CreateTestConfig();
        await _repository.AddAsync(config);

        var newName = Domain.ProviderManagement.ProviderName.From("Updated Provider");
        config.Update(
            newName,
            config.AuthorizationEndpoint,
            config.TokenEndpoint,
            config.ClientId,
            config.RedirectUri,
            null,
            config.DefaultScopes,
            config.SupportedGrantTypes);

        await _repository.UpdateAsync(config);

        var result = await _repository.GetByIdAsync(config.Id);
        result!.Name.Value.Should().Be("Updated Provider");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveConfig()
    {
        var config = CreateTestConfig();
        await _repository.AddAsync(config);

        await _repository.DeleteAsync(config.Id);

        var result = await _repository.GetByIdAsync(config.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAllConfigs()
    {
        await _repository.AddAsync(CreateTestConfig("Provider 1"));
        await _repository.AddAsync(CreateTestConfig("Provider 2"));

        var results = await _repository.ListAsync();

        results.Should().HaveCount(2);
    }

    private static Domain.ProviderManagement.OAuthProviderConfig CreateTestConfig(string name = "Test Provider")
    {
        return new Domain.ProviderManagement.OAuthProviderConfig(
            Domain.ProviderManagement.ProviderId.New(),
            Domain.ProviderManagement.ProviderName.From(name),
            Domain.ProviderManagement.OAuthEndpoint.From("https://auth.example.com"),
            Domain.ProviderManagement.OAuthEndpoint.From("https://token.example.com"),
            Domain.ProviderManagement.ClientId.From("client123"),
            Domain.ProviderManagement.RedirectUri.From("https://app.example.com/callback"));
    }
}
