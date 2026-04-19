using MongoDB.Driver;

namespace CustomLogin.Infrastructure.Persistence;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient client, string databaseName)
    {
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name) =>
        _database.GetCollection<T>(name);
}
