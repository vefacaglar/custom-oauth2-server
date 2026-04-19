using MongoDB.Driver;
using CustomLogin.Application.OAuthFlows;
using CustomLogin.Domain.OAuthFlows;
using CustomLogin.Infrastructure.Persistence.Documents;

namespace CustomLogin.Infrastructure.Persistence.Repositories;

public sealed class FlowSessionRepository : IFlowSessionRepository
{
    private readonly IMongoCollection<FlowSessionDocument> _collection;

    public FlowSessionRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<FlowSessionDocument>(MongoCollectionNames.FlowSessions);
    }

    public async Task<OAuthFlowSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var filter = Builders<FlowSessionDocument>.Filter.Eq(d => d.SessionId, id.ToString());
        var document = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task AddAsync(OAuthFlowSession session, CancellationToken ct = default)
    {
        var document = FlowSessionDocument.FromDomain(session);
        await _collection.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task UpdateAsync(OAuthFlowSession session, CancellationToken ct = default)
    {
        var filter = Builders<FlowSessionDocument>.Filter.Eq(d => d.SessionId, session.Id.ToString());
        var existing = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        if (existing is null)
            return;

        var document = FlowSessionDocument.FromDomain(session);
        document.Id = existing.Id;
        await _collection.ReplaceOneAsync(filter, document, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<OAuthFlowSession>> ListAsync(CancellationToken ct = default)
    {
        var documents = await _collection.Find(_ => true)
            .SortByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
        return documents.Select(d => d.ToDomain()).ToList();
    }
}
