using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class DispatcherExtensions
{
    public static IServiceCollection AddDispatcher(this IServiceCollection services)
    {
        services.AddScoped<CustomLogin.Application.Dispatcher.IDispatcher, CustomLogin.Application.Dispatcher.MediatorDispatcher>();
        return services;
    }
}
