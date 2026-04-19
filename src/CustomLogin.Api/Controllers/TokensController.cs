using CustomLogin.Application.Dispatcher;
using CustomLogin.Application.TokenInspection.Commands;
using CustomLogin.Application.TokenInspection.Queries;
using CustomLogin.Contracts.TokenInspection;
using Microsoft.AspNetCore.Mvc;

namespace CustomLogin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TokensController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public TokensController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost("exchange-code")]
    public async Task<IActionResult> ExchangeCode(
        [FromBody] ExchangeAuthorizationCodeRequest request,
        CancellationToken ct)
    {
        var command = new ExchangeAuthorizationCodeCommand { FlowSessionId = request.FlowSessionId };
        var result = await _dispatcher.Send(command, ct);

        if (!result.IsSuccess)
            return result.Error switch
            {
                "Flow session not found." or "Provider config not found." => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };

        return Ok(result.Value);
    }

    [HttpPost("decode")]
    public async Task<IActionResult> DecodeJwt(
        [FromBody] DecodeJwtRequest request,
        CancellationToken ct)
    {
        var command = new DecodeJwtCommand { Token = request.Token };
        var result = await _dispatcher.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("responses/{id:guid}")]
    public async Task<IActionResult> GetTokenResponse(Guid id, CancellationToken ct)
    {
        var query = new GetTokenResponseByIdQuery { Id = id };
        var result = await _dispatcher.Query(query, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("client-credentials")]
    public async Task<IActionResult> ClientCredentials(
        [FromBody] ClientCredentialsRequest request,
        CancellationToken ct)
    {
        var command = new ExecuteClientCredentialsCommand
        {
            ProviderId = request.ProviderId,
            Scope = request.Scope
        };

        var result = await _dispatcher.Send(command, ct);

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
        CancellationToken ct)
    {
        var command = new RefreshAccessTokenCommand
        {
            FlowSessionId = id,
            RefreshToken = request.RefreshToken
        };

        var result = await _dispatcher.Send(command, ct);

        if (!result.IsSuccess)
            return result.Error switch
            {
                "Flow session not found." or "Provider config not found." => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };

        return Ok(result.Value);
    }
}
