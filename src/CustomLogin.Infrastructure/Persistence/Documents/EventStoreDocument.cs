using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CustomLogin.Infrastructure.Persistence.Documents;

public sealed class EventStoreDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("streamId")]
    public string StreamId { get; set; } = string.Empty;

    [BsonElement("streamType")]
    public string StreamType { get; set; } = string.Empty;

    [BsonElement("version")]
    public int Version { get; set; }

    [BsonElement("eventType")]
    public string EventType { get; set; } = string.Empty;

    [BsonElement("payload")]
    public BsonDocument Payload { get; set; } = new();

    [BsonElement("occurredAt")]
    public DateTime OccurredAt { get; set; }
}
