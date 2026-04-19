using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using OAuthLab.Application.Configuration;
using OAuthLab.Application.ProviderManagement;
using OAuthLab.Infrastructure.Persistence;
using OAuthLab.Infrastructure.Persistence.Repositories;

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

        services.AddScoped<IProviderConfigRepository, ProviderConfigRepository>();

        return services;
    }
}
