using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using CustomLogin.Application.OAuthFlows;
using CustomLogin.Domain.OAuthFlows;
using CustomLogin.Infrastructure.Persistence.Documents;
using System.Text.Json;

namespace CustomLogin.Infrastructure.Persistence.EventSourcing;

public sealed class MongoEventStore : IEventStore
{
    private readonly IMongoCollection<EventStoreDocument> _collection;

    public MongoEventStore(MongoDbContext context)
    {
        _collection = context.GetCollection<EventStoreDocument>(MongoCollectionNames.EventStore);
    }

    public async Task AppendEventsAsync(Guid streamId, string streamType, IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        var lastVersion = await GetLastVersionAsync(streamId, ct);

        var documents = new List<EventStoreDocument>();
        foreach (var e in events)
        {
            lastVersion++;
            var json = JsonSerializer.Serialize(e);
            var doc = new EventStoreDocument
            {
                StreamId = streamId.ToString(),
                StreamType = streamType,
                Version = lastVersion,
                EventType = e.GetType().Name,
                Payload = BsonDocument.Parse(json),
                OccurredAt = e.OccurredAt
            };
            documents.Add(doc);
        }

        if (documents.Count > 0)
            await _collection.InsertManyAsync(documents, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<IDomainEvent>> GetEventsAsync(Guid streamId, CancellationToken ct = default)
    {
        var filter = Builders<EventStoreDocument>.Filter.Eq(d => d.StreamId, streamId.ToString());
        var documents = await _collection.Find(filter)
            .SortBy(d => d.Version)
            .ToListAsync(ct);

        return documents.Select(MapToDomainEvent).ToList();
    }

    private async Task<int> GetLastVersionAsync(Guid streamId, CancellationToken ct)
    {
        var filter = Builders<EventStoreDocument>.Filter.Eq(d => d.StreamId, streamId.ToString());
        var last = await _collection.Find(filter)
            .SortByDescending(d => d.Version)
            .Limit(1)
            .FirstOrDefaultAsync(ct);

        return last?.Version ?? 0;
    }

    private static IDomainEvent MapToDomainEvent(EventStoreDocument doc)
    {
        return doc.EventType switch
        {
            nameof(OAuthFlowSessionStartedEvent) => JsonSerializer.Deserialize<OAuthFlowSessionStartedEvent>(doc.Payload.ToJson())!,
            nameof(PkceChallengeGeneratedEvent) => JsonSerializer.Deserialize<PkceChallengeGeneratedEvent>(doc.Payload.ToJson())!,
            nameof(AuthorizationUrlGeneratedEvent) => JsonSerializer.Deserialize<AuthorizationUrlGeneratedEvent>(doc.Payload.ToJson())!,
            nameof(OAuthCallbackReceivedEvent) => JsonSerializer.Deserialize<OAuthCallbackReceivedEvent>(doc.Payload.ToJson())!,
            nameof(AuthorizationCodeExchangedForTokenEvent) => JsonSerializer.Deserialize<AuthorizationCodeExchangedForTokenEvent>(doc.Payload.ToJson())!,
            nameof(OAuthFlowSessionFailedEvent) => JsonSerializer.Deserialize<OAuthFlowSessionFailedEvent>(doc.Payload.ToJson())!,
            nameof(OAuthFlowSessionExpiredEvent) => JsonSerializer.Deserialize<OAuthFlowSessionExpiredEvent>(doc.Payload.ToJson())!,
            _ => throw new InvalidOperationException($"Unknown event type: {doc.EventType}")
        };
    }
}
