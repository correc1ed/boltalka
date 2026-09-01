using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using boltalka.Application.Abstractions.Services;
using boltalka.Contracts.Models.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace boltalka.WebApi.Controllers;

/// <summary>
/// Управление чатами.
/// </summary>
[ApiController]
[Route("api/v1/chats")]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IMapper _mapper;

    public ChatsController(IChatService chatService, IMapper mapper)
    {
        _chatService = chatService;
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
    /// Создать личный чат.
    /// </summary>
    [HttpPost("private")]
    public async Task<IActionResult> CreatePrivateChatAsync(
        [FromBody] CreatePrivateChatContract createPrivateChat,
        CancellationToken cancellationToken)
    {
        var creatorUserId = GetCurrentUserId();
        var result = await _chatService.CreatePrivateChatAsync(creatorUserId, createPrivateChat.OtherUserId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<ChatInfoContract>(result.Value));
    }

    /// <summary>
    /// Создать групповой чат.
    /// </summary>
    [HttpPost("group")]
    public async Task<IActionResult> CreateGroupChatAsync(
        [FromBody] CreateGroupChatContract createGroupChat,
        CancellationToken cancellationToken)
    {
        var creatorUserId = GetCurrentUserId();
        var result = await _chatService.CreateGroupChatAsync(
            creatorUserId,
            createGroupChat.Name,
            createGroupChat.MemberIds,
            cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<ChatInfoContract>(result.Value));
    }

    /// <summary>
    /// Получить информацию о чате.
    /// </summary>
    [HttpGet("{chatId:guid}")]
    public async Task<ActionResult<ChatContract>> GetChatAsync(Guid chatId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _chatService.GetChatAsync(chatId, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<ChatInfoContract>(result.Value));
    }

    /// <summary>
    /// Получить список чатов текущего пользователя.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatListItemContract>>> GetMyChatsAsync(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _chatService.GetUserChatsAsync(userId, skip, take, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<IEnumerable<ChatListItemContract>>(result.Value));
    }

    /// <summary>
    /// Добавить участника в чат.
    /// </summary>
    [HttpPost("{chatId:guid}/members")]
    public async Task<IActionResult> AddMemberAsync(
        Guid chatId,
        [FromBody] Guid newMemberId,
        CancellationToken cancellationToken)
    {
        var requesterId = GetCurrentUserId();
        var result = await _chatService.AddMemberAsync(chatId, requesterId, newMemberId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }

    /// <summary>
    /// Удалить участника из чата.
    /// </summary>
    [HttpDelete("{chatId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMemberAsync(
        Guid chatId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var requesterId = GetCurrentUserId();
        var result = await _chatService.RemoveMemberAsync(chatId, requesterId, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }

    /// <summary>
    /// Изменить роль участника в чате.
    /// </summary>
    [HttpPatch("{chatId:guid}/members/{userId:guid}/role")]
    public async Task<IActionResult> ChangeMemberRoleAsync(
        Guid chatId,
        Guid userId,
        [FromBody] MemberRole newRole,
        CancellationToken cancellationToken)
    {
        var requesterId = GetCurrentUserId();
        var result = await _chatService.ChangeMemberRoleAsync(chatId, requesterId, userId, (Application.Enums.ChatMember.MemberRole)newRole, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }

    /// <summary>
    /// Обновить название чата.
    /// </summary>
    [HttpPatch("{chatId:guid}/name")]
    public async Task<IActionResult> UpdateChatNameAsync(
        Guid chatId,
        [FromBody] string newName,
        CancellationToken cancellationToken)
    {
        var requesterId = GetCurrentUserId();
        var result = await _chatService.UpdateChatNameAsync(chatId, requesterId, newName, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }
}