using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models.Message;
using boltalka.Contracts.Models.Message;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace boltalka.WebApi.Controllers;

/// <summary>
/// Управление сообщениями.
/// </summary>
[ApiController]
[Route("api/v1")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IMapper _mapper;

    public MessagesController(IMessageService messageService, IMapper mapper)
    {
        _messageService = messageService;
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
    /// Отправить сообщение в чат.
    /// </summary>
    [HttpPost("chats/{chatId:guid}/messages")]
    public async Task<IActionResult> SendMessageAsync(
        Guid chatId,
        [FromBody] SendMessageContract sendMessage,
        CancellationToken cancellationToken)
    {
        var senderId = GetCurrentUserId();

        // Преобразуем контракт в модель, которую ожидает сервис
        var message = _mapper.Map<SendMessage>(sendMessage);
        // Устанавливаем ChatId из маршрута, чтобы исключить расхождения
        message.ChatId = chatId;

        var result = await _messageService.SendMessageAsync(senderId, message, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<MessageInfoContract>(result.Value));
    }

    /// <summary>
    /// Получить сообщения чата.
    /// </summary>
    [HttpGet("chats/{chatId:guid}/messages")]
    public async Task<ActionResult<IEnumerable<MessageContract>>> GetMessagesAsync(
        Guid chatId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] DateTime? before = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var result = await _messageService.GetMessagesAsync(chatId, userId, skip, take, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<IEnumerable<MessageContract>>(result.Value));
    }

    /// <summary>
    /// Отметить сообщение как прочитанное.
    /// </summary>
    [HttpPut("messages/{messageId:guid}/read")]
    public async Task<IActionResult> MarkAsReadAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var result = await _messageService.MarkAsReadAsync(messageId, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }

    /// <summary>
    /// Редактировать сообщение.
    /// </summary>
    [HttpPut("messages/{messageId:guid}")]
    public async Task<IActionResult> EditMessageAsync(
        Guid messageId,
        [FromBody] string newText,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var result = await _messageService.EditMessageAsync(messageId, userId, newText, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<MessageInfoContract>(result.Value));
    }

    /// <summary>
    /// Удалить сообщение.
    /// </summary>
    [HttpDelete("messages/{messageId:guid}")]
    public async Task<IActionResult> DeleteMessageAsync(Guid messageId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var result = await _messageService.DeleteMessageAsync(messageId, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }
}