using CustomLogin.Application.Dispatcher;
using CustomLogin.Application.OAuthFlows.Commands;
using CustomLogin.Application.OAuthFlows.Queries;
using CustomLogin.Contracts.OAuthFlows;
using Microsoft.AspNetCore.Mvc;

namespace CustomLogin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OAuthFlowsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public OAuthFlowsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost("authorization-code-pkce/start")]
    public async Task<IActionResult> StartAuthorizationCodePkce(
        [FromBody] StartAuthorizationCodePkceRequest request,
        CancellationToken ct)
    {
        var command = new StartAuthorizationCodePkceFlowCommand { ProviderId = request.ProviderId };
        var result = await _dispatcher.Send(command, ct);

        if (!result.IsSuccess)
            return result.Error == "Provider config not found."
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> HandleCallback(
        [FromQuery] Guid sessionId,
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        [FromQuery] string? error_description,
        CancellationToken ct)
    {
        var command = new HandleOAuthCallbackCommand
        {
            SessionId = sessionId,
            Code = code,
            State = state,
            Error = error,
            ErrorDescription = error_description
        };

        var result = await _dispatcher.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("callback")]
    public async Task<IActionResult> HandleCallbackPost(
        [FromBody] HandleOAuthCallbackRequest request,
        CancellationToken ct)
    {
        var command = new HandleOAuthCallbackCommand
        {
            SessionId = request.SessionId,
            Code = request.Code,
            State = request.State,
            Error = request.Error,
            ErrorDescription = request.ErrorDescription
        };

        var result = await _dispatcher.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetFlowSessionByIdQuery { Id = id };
        var result = await _dispatcher.Query(query, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var query = new ListFlowSessionsQuery();
        var result = await _dispatcher.Query(query, ct);
        return Ok(result.Value);
    }
}
