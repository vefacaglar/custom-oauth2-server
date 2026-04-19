using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using CustomLogin.Application.ProviderManagement.Commands;
using CustomLogin.Application.ProviderManagement.Queries;
using CustomLogin.Contracts.ProviderManagement;

namespace CustomLogin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProviderConfigsController : ControllerBase
{
    private readonly ILogger<ProviderConfigsController> _logger;

    public ProviderConfigsController(ILogger<ProviderConfigsController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProviderConfigRequest request,
        [FromServices] IValidator<CreateProviderConfigCommand> validator,
        [FromServices] CreateProviderConfigCommandHandler handler,
        CancellationToken ct)
    {
        var command = new CreateProviderConfigCommand
        {
            Name = request.Name,
            AuthorizationEndpoint = request.AuthorizationEndpoint,
            TokenEndpoint = request.TokenEndpoint,
            RevocationEndpoint = request.RevocationEndpoint,
            IntrospectionEndpoint = request.IntrospectionEndpoint,
            UserInfoEndpoint = request.UserInfoEndpoint,
            Issuer = request.Issuer,
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,
            RedirectUri = request.RedirectUri,
            DefaultScopes = request.DefaultScopes,
            SupportedGrantTypes = request.SupportedGrantTypes
        };

        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        var result = await handler.Handle(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] GetProviderConfigByIdQueryHandler handler,
        CancellationToken ct)
    {
        var query = new GetProviderConfigByIdQuery { Id = id };
        var result = await handler.Handle(query, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromServices] ListProviderConfigsQueryHandler handler,
        CancellationToken ct)
    {
        var query = new ListProviderConfigsQuery();
        var result = await handler.Handle(query, ct);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProviderConfigRequest request,
        [FromServices] IValidator<UpdateProviderConfigCommand> validator,
        [FromServices] UpdateProviderConfigCommandHandler handler,
        CancellationToken ct)
    {
        var command = new UpdateProviderConfigCommand
        {
            Id = id,
            Name = request.Name,
            AuthorizationEndpoint = request.AuthorizationEndpoint,
            TokenEndpoint = request.TokenEndpoint,
            RevocationEndpoint = request.RevocationEndpoint,
            IntrospectionEndpoint = request.IntrospectionEndpoint,
            UserInfoEndpoint = request.UserInfoEndpoint,
            Issuer = request.Issuer,
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,
            RedirectUri = request.RedirectUri,
            DefaultScopes = request.DefaultScopes,
            SupportedGrantTypes = request.SupportedGrantTypes
        };

        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        var result = await handler.Handle(command, ct);

        if (!result.IsSuccess)
            return result.Error == "Provider config not found."
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] DeleteProviderConfigCommandHandler handler,
        CancellationToken ct)
    {
        var command = new DeleteProviderConfigCommand { Id = id };
        var result = await handler.Handle(command, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}
