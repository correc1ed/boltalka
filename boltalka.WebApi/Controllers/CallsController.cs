using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using boltalka.Application.Abstractions.Services;
using boltalka.Contracts.Models.Call;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace boltalka.WebApi.Controllers;

/// <summary>
/// Управление звонками.
/// </summary>
[ApiController]
[Route("api/v1")]
[Authorize]
public class CallsController : ControllerBase
{
    private readonly ICallService _callService;
    private readonly IMapper _mapper;

    public CallsController(ICallService callService, IMapper mapper)
    {
        _callService = callService;
        _mapper = mapper;
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("Пользователь не авторизован.");
        return userId;
    }

    /// <summary>
    /// Начать звонок в чате.
    /// </summary>
    [HttpPost("chats/{chatId:guid}/calls")]
    public async Task<IActionResult> StartCallAsync(
        Guid chatId,
        [FromBody] CallType callType,
        CancellationToken cancellationToken)
    {
        var initiatorId = GetCurrentUserId();
        var result = await _callService.StartCallAsync(chatId, initiatorId, (boltalka.Application.Enums.Call.CallType)callType, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<CallContract>(result.Value));
    }

    /// <summary>
    /// Принять входящий звонок.
    /// </summary>
    [HttpPost("calls/{callId:guid}/accept")]
    public async Task<IActionResult> AcceptCallAsync(Guid callId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _callService.AcceptCallAsync(callId, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<CallContract>(result.Value));
    }

    /// <summary>
    /// Завершить звонок.
    /// </summary>
    [HttpPost("calls/{callId:guid}/end")]
    public async Task<IActionResult> EndCallAsync(Guid callId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _callService.EndCallAsync(callId, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<CallContract>(result.Value));
    }

    /// <summary>
    /// Отклонить звонок.
    /// </summary>
    [HttpPost("calls/{callId:guid}/decline")]
    public async Task<IActionResult> DeclineCallAsync(Guid callId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _callService.DeclineCallAsync(callId, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }

    /// <summary>
    /// Получить активный звонок в чате.
    /// </summary>
    [HttpGet("chats/{chatId:guid}/calls/active")]
    public async Task<ActionResult<CallContract?>> GetActiveCallAsync(
        Guid chatId,
        CancellationToken cancellationToken)
    {
        // Проверка членства не выполняется в сервисе, но можно добавить здесь
        var result = await _callService.GetActiveCallAsync(chatId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value is null ? null : _mapper.Map<CallContract>(result.Value));
    }

    /// <summary>
    /// Получить историю звонков в чате.
    /// </summary>
    [HttpGet("chats/{chatId:guid}/calls/history")]
    public async Task<ActionResult<IEnumerable<CallContract>>> GetCallHistoryAsync(
        Guid chatId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _callService.GetCallHistoryAsync(chatId, userId, skip, take, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<IEnumerable<CallContract>>(result.Value));
    }
}