using FluentValidation;
using OAuthLab.Application.ProviderManagement.Commands;
using OAuthLab.Application.ProviderManagement.Queries;
using OAuthLab.Application.ProviderManagement.Validators;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateProviderConfigCommandHandler>();
        services.AddScoped<UpdateProviderConfigCommandHandler>();
        services.AddScoped<DeleteProviderConfigCommandHandler>();
        services.AddScoped<GetProviderConfigByIdQueryHandler>();
        services.AddScoped<ListProviderConfigsQueryHandler>();

        services.AddTransient<IValidator<CreateProviderConfigCommand>, CreateProviderConfigCommandValidator>();
        services.AddTransient<IValidator<UpdateProviderConfigCommand>, UpdateProviderConfigCommandValidator>();

        return services;
    }
}
