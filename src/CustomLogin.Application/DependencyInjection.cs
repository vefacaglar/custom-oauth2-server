using CustomLogin.Application.Dispatcher;
using CustomLogin.Application.OAuthFlows.Commands;
using CustomLogin.Application.OAuthFlows.Queries;
using CustomLogin.Application.ProviderManagement.Commands;
using CustomLogin.Application.ProviderManagement.Queries;
using CustomLogin.Application.ProviderManagement.Validators;
using CustomLogin.Application.TokenInspection.Commands;
using CustomLogin.Application.TokenInspection.Queries;
using CustomLogin.Contracts.OAuthFlows;
using CustomLogin.Contracts.ProviderManagement;
using CustomLogin.Contracts.TokenInspection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddDispatcher();

        services.AddScoped<ICommandHandler<CreateProviderConfigCommand, Guid>, CreateProviderConfigCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProviderConfigCommand>, UpdateProviderConfigCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteProviderConfigCommand>, DeleteProviderConfigCommandHandler>();
        services.AddScoped<IQueryHandler<GetProviderConfigByIdQuery, ProviderConfigResponse>, GetProviderConfigByIdQueryHandler>();
        services.AddScoped<IQueryHandler<ListProviderConfigsQuery, IReadOnlyList<ProviderConfigResponse>>, ListProviderConfigsQueryHandler>();

        services.AddScoped<ICommandHandler<StartAuthorizationCodePkceFlowCommand, StartAuthorizationCodePkceResponse>, StartAuthorizationCodePkceFlowCommandHandler>();
        services.AddScoped<ICommandHandler<HandleOAuthCallbackCommand, FlowSessionResponse>, HandleOAuthCallbackCommandHandler>();
        services.AddScoped<IQueryHandler<GetFlowSessionByIdQuery, FlowSessionResponse>, GetFlowSessionByIdQueryHandler>();
        services.AddScoped<IQueryHandler<ListFlowSessionsQuery, IReadOnlyList<FlowSessionResponse>>, ListFlowSessionsQueryHandler>();

        services.AddScoped<ICommandHandler<ExchangeAuthorizationCodeCommand, TokenExchangeResponse>, ExchangeAuthorizationCodeCommandHandler>();
        services.AddScoped<ICommandHandler<ExecuteClientCredentialsCommand, TokenExchangeResponse>, ExecuteClientCredentialsCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshAccessTokenCommand, TokenExchangeResponse>, RefreshAccessTokenCommandHandler>();
        services.AddScoped<ICommandHandler<DecodeJwtCommand, DecodedJwtResponse>, DecodeJwtCommandHandler>();
        services.AddScoped<IQueryHandler<GetTokenResponseByIdQuery, TokenResponseSummary>, GetTokenResponseByIdQueryHandler>();

        services.AddTransient<IValidator<CreateProviderConfigCommand>, CreateProviderConfigCommandValidator>();
        services.AddTransient<IValidator<UpdateProviderConfigCommand>, UpdateProviderConfigCommandValidator>();

        return services;
    }
}
