using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using CustomLogin.Application.Configuration;
using CustomLogin.Application.OAuthFlows;
using CustomLogin.Application.ProviderManagement;
using CustomLogin.Infrastructure.OAuthFlows;
using CustomLogin.Infrastructure.Persistence;
using CustomLogin.Infrastructure.Persistence.EventSourcing;
using CustomLogin.Infrastructure.Persistence.Repositories;

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
        services.AddScoped<IFlowSessionRepository, FlowSessionRepository>();
        services.AddScoped<IEventStore, MongoEventStore>();
        services.AddScoped<IPkceService, PkceService>();

        return services;
    }
}
