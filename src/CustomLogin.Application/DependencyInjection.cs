using FluentValidation;
using CustomLogin.Application.OAuthFlows;
using CustomLogin.Application.OAuthFlows.Commands;
using CustomLogin.Application.OAuthFlows.Queries;
using CustomLogin.Application.ProviderManagement.Commands;
using CustomLogin.Application.ProviderManagement.Queries;
using CustomLogin.Application.ProviderManagement.Validators;

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

        services.AddScoped<StartAuthorizationCodePkceFlowCommandHandler>();
        services.AddScoped<HandleOAuthCallbackCommandHandler>();
        services.AddScoped<GetFlowSessionByIdQueryHandler>();
        services.AddScoped<ListFlowSessionsQueryHandler>();

        services.AddTransient<IValidator<CreateProviderConfigCommand>, CreateProviderConfigCommandValidator>();
        services.AddTransient<IValidator<UpdateProviderConfigCommand>, UpdateProviderConfigCommandValidator>();

        return services;
    }
}
