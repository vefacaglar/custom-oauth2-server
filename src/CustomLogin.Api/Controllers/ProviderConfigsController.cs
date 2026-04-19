using CustomLogin.Application.Dispatcher;
using CustomLogin.Application.ProviderManagement.Commands;
using CustomLogin.Application.ProviderManagement.Queries;
using CustomLogin.Contracts.ProviderManagement;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CustomLogin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProviderConfigsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    private readonly IValidator<CreateProviderConfigCommand> _createValidator;
    private readonly IValidator<UpdateProviderConfigCommand> _updateValidator;

    public ProviderConfigsController(
        IDispatcher dispatcher,
        IValidator<CreateProviderConfigCommand> createValidator,
        IValidator<UpdateProviderConfigCommand> updateValidator)
    {
        _dispatcher = dispatcher;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProviderConfigRequest request,
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

        var validationResult = await _createValidator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList() });

        var result = await _dispatcher.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetProviderConfigByIdQuery { Id = id };
        var result = await _dispatcher.Query(query, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var query = new ListProviderConfigsQuery();
        var result = await _dispatcher.Query(query, ct);
        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProviderConfigRequest request,
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

        var validationResult = await _updateValidator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList() });

        var result = await _dispatcher.Send(command, ct);

        if (!result.IsSuccess)
            return result.Error == "Provider config not found."
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var command = new DeleteProviderConfigCommand { Id = id };
        var result = await _dispatcher.Send(command, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}
