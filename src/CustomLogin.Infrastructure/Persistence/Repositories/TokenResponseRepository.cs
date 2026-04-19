using MongoDB.Driver;
using CustomLogin.Application.TokenInspection;
using CustomLogin.Domain.TokenInspection;
using CustomLogin.Infrastructure.Persistence.Documents;

namespace CustomLogin.Infrastructure.Persistence.Repositories;

public sealed class TokenResponseRepository : ITokenResponseRepository
{
    private readonly IMongoCollection<TokenResponseDocument> _collection;

    public TokenResponseRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<TokenResponseDocument>(MongoCollectionNames.TokenResponses);
    }

    public async Task<TokenResponseRecord?> GetByIdAsync(TokenResponseId id, CancellationToken ct = default)
    {
        var filter = Builders<TokenResponseDocument>.Filter.Eq(d => d.TokenResponseId, id.Value.ToString());
        var document = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<IReadOnlyList<TokenResponseRecord>> GetByFlowSessionIdAsync(Guid flowSessionId, CancellationToken ct = default)
    {
        var filter = Builders<TokenResponseDocument>.Filter.Eq(d => d.FlowSessionId, flowSessionId.ToString());
        var documents = await _collection.Find(filter).ToListAsync(ct);
        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task AddAsync(TokenResponseRecord record, CancellationToken ct = default)
    {
        var document = TokenResponseDocument.FromDomain(record);
        await _collection.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<TokenResponseRecord>> ListAsync(CancellationToken ct = default)
    {
        var documents = await _collection.Find(_ => true)
            .SortByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
        return documents.Select(d => d.ToDomain()).ToList();
    }
}
