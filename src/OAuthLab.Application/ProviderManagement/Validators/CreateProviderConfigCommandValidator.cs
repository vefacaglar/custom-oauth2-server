using FluentValidation;
using OAuthLab.Application.ProviderManagement.Commands;

namespace OAuthLab.Application.ProviderManagement.Validators;

public sealed class CreateProviderConfigCommandValidator : AbstractValidator<CreateProviderConfigCommand>
{
    public CreateProviderConfigCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Provider name is required.");

        RuleFor(x => x.AuthorizationEndpoint)
            .NotEmpty().WithMessage("Authorization endpoint is required.")
            .Must(BeAValidUri).WithMessage("Authorization endpoint must be a valid URL.");

        RuleFor(x => x.TokenEndpoint)
            .NotEmpty().WithMessage("Token endpoint is required.")
            .Must(BeAValidUri).WithMessage("Token endpoint must be a valid URL.");

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required.");

        RuleFor(x => x.RedirectUri)
            .NotEmpty().WithMessage("Redirect URI is required.")
            .Must(BeAValidUri).WithMessage("Redirect URI must be a valid URL.");

        RuleFor(x => x.RevocationEndpoint)
            .Must(BeAValidUriOrNull).WithMessage("Revocation endpoint must be a valid URL.");

        RuleFor(x => x.IntrospectionEndpoint)
            .Must(BeAValidUriOrNull).WithMessage("Introspection endpoint must be a valid URL.");

        RuleFor(x => x.UserInfoEndpoint)
            .Must(BeAValidUriOrNull).WithMessage("UserInfo endpoint must be a valid URL.");

        RuleFor(x => x.Issuer)
            .Must(BeAValidUriOrNull).WithMessage("Issuer must be a valid URL.");
    }

    private static bool BeAValidUri(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }

    private static bool BeAValidUriOrNull(string? value)
    {
        if (value is null)
            return true;

        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }
}
