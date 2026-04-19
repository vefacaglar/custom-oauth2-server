using MongoDB.Driver;
using OAuthLab.Application.ProviderManagement;
using OAuthLab.Domain.ProviderManagement;
using OAuthLab.Infrastructure.Persistence.Documents;

namespace OAuthLab.Infrastructure.Persistence.Repositories;

public sealed class ProviderConfigRepository : IProviderConfigRepository
{
    private readonly IMongoCollection<ProviderConfigDocument> _collection;

    public ProviderConfigRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<ProviderConfigDocument>(MongoCollectionNames.ProviderConfigs);
    }

    public async Task<OAuthProviderConfig?> GetByIdAsync(ProviderId id, CancellationToken ct = default)
    {
        var filter = Builders<ProviderConfigDocument>.Filter.Eq(d => d.ProviderId, id.Value.ToString());
        var document = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<bool> ExistsByNameAsync(ProviderName name, CancellationToken ct = default)
    {
        var filter = Builders<ProviderConfigDocument>.Filter.Eq(d => d.Name, name.Value);
        var count = await _collection.CountDocumentsAsync(filter, new CountOptions { Limit = 1 }, ct);
        return count > 0;
    }

    public async Task AddAsync(OAuthProviderConfig config, CancellationToken ct = default)
    {
        var document = ProviderConfigDocument.FromDomain(config);
        await _collection.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task UpdateAsync(OAuthProviderConfig config, CancellationToken ct = default)
    {
        var filter = Builders<ProviderConfigDocument>.Filter.Eq(d => d.ProviderId, config.Id.Value.ToString());
        var existing = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        if (existing is null)
            return;

        var document = ProviderConfigDocument.FromDomain(config);
        document.Id = existing.Id;
        await _collection.ReplaceOneAsync(filter, document, cancellationToken: ct);
    }

    public async Task DeleteAsync(ProviderId id, CancellationToken ct = default)
    {
        var filter = Builders<ProviderConfigDocument>.Filter.Eq(d => d.ProviderId, id.Value.ToString());
        await _collection.DeleteOneAsync(filter, ct);
    }

    public async Task<IReadOnlyList<OAuthProviderConfig>> ListAsync(CancellationToken ct = default)
    {
        var documents = await _collection.Find(_ => true).ToListAsync(ct);
        return documents.Select(d => d.ToDomain()).ToList();
    }
}
