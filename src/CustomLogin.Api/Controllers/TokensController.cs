using Microsoft.AspNetCore.Mvc;
using CustomLogin.Application.TokenInspection.Commands;
using CustomLogin.Application.TokenInspection.Queries;
using CustomLogin.Contracts.TokenInspection;

namespace CustomLogin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TokensController : ControllerBase
{
    [HttpPost("exchange-code")]
    public async Task<IActionResult> ExchangeCode(
        [FromBody] ExchangeAuthorizationCodeRequest request,
        [FromServices] ExchangeAuthorizationCodeCommandHandler handler,
        CancellationToken ct)
    {
        var command = new ExchangeAuthorizationCodeCommand
        {
            FlowSessionId = request.FlowSessionId
        };

        var result = await handler.Handle(command, ct);

        if (!result.IsSuccess)
            return result.Error switch
            {
                "Flow session not found." or "Provider config not found." => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };

        return Ok(result.Value);
    }

    [HttpPost("decode")]
    public IActionResult DecodeJwt(
        [FromBody] DecodeJwtRequest request,
        [FromServices] DecodeJwtCommandHandler handler)
    {
        var command = new DecodeJwtCommand { Token = request.Token };
        var result = handler.Handle(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("responses/{id:guid}")]
    public async Task<IActionResult> GetTokenResponse(
        Guid id,
        [FromServices] GetTokenResponseByIdQueryHandler handler,
        CancellationToken ct)
    {
        var query = new GetTokenResponseByIdQuery { Id = id };
        var result = await handler.Handle(query, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("client-credentials")]
    public async Task<IActionResult> ClientCredentials(
        [FromBody] ClientCredentialsRequest request,
        [FromServices] ExecuteClientCredentialsCommandHandler handler,
        CancellationToken ct)
    {
        var command = new ExecuteClientCredentialsCommand
        {
            ProviderId = request.ProviderId,
            Scope = request.Scope
        };

        var result = await handler.Handle(command, ct);

        if (!result.IsSuccess)
            return result.Error switch
            {
                "Provider config not found." => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/refresh-token")]
    public async Task<IActionResult> RefreshToken(
        Guid id,
        [FromBody] RefreshTokenRequest request,
        [FromServices] RefreshAccessTokenCommandHandler handler,
        CancellationToken ct)
    {
        var command = new RefreshAccessTokenCommand
        {
            FlowSessionId = id,
            RefreshToken = request.RefreshToken
        };

        var result = await handler.Handle(command, ct);

        if (!result.IsSuccess)
            return result.Error switch
            {
                "Flow session not found." or "Provider config not found." => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };

        return Ok(result.Value);
    }
}
