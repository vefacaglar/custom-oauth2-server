using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OAuthLab.Application.Configuration;
using OAuthLab.Infrastructure.Persistence;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDBSettings>(configuration.GetSection(MongoDBSettings.SectionName));

        var settings = configuration.GetSection(MongoDBSettings.SectionName).Get<MongoDBSettings>()
            ?? throw new InvalidOperationException("MongoDB settings not configured.");

        services.AddSingleton<IMongoClient>(_ => new MongoClient(settings.ConnectionString));
        services.AddSingleton<MongoDbContext>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return new MongoDbContext(client, settings.DatabaseName);
        });

        return services;
    }
}
