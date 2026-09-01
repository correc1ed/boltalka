using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using boltalka.Application.Abstractions.Services;
using boltalka.Contracts.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace boltalka.WebApi.Controllers;

/// <summary>
/// Управление пользователями.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public UsersController(IUserService userService, IMapper mapper)
    {
        _userService = userService;
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
    /// Получить профиль текущего пользователя.
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileContract>> GetMyProfileAsync(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.GetProfileAsync(userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<UserProfileContract>(result.Value));
    }

    /// <summary>
    /// Получить пользователя по идентификатору.
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserProfileContract>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _userService.GetUserByIdAsync(userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<UserProfileContract>(result.Value));
    }

    /// <summary>
    /// Обновить отображаемое имя текущего пользователя.
    /// </summary>
    [HttpPut("me/display-name")]
    public async Task<ActionResult<UserProfileContract>> UpdateDisplayNameAsync(
        [FromBody] string newDisplayName,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.UpdateDisplayNameAsync(userId, newDisplayName, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<UserProfileContract>(result.Value));
    }

    /// <summary>
    /// Установить аватар текущего пользователя.
    /// </summary>
    [HttpPut("me/avatar")]
    public async Task<ActionResult<UserProfileContract>> SetAvatarAsync(
        [FromBody] Guid mediaId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.SetAvatarAsync(userId, mediaId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<UserProfileContract>(result.Value));
    }

    /// <summary>
    /// Поиск пользователей по строке запроса.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<UserProfileContract>>> SearchUsersAsync(
        [FromQuery] string query,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { error = "Поисковый запрос не может быть пустым." });

        var result = await _userService.SearchUsersAsync(query, skip, take, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<IEnumerable<UserProfileContract>>(result.Value));
    }

    /// <summary>
    /// Деактивировать пользователя.
    /// </summary>
    [HttpPatch("{userId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _userService.DeactivateUserAsync(userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }

    /// <summary>
    /// Активировать пользователя.
    /// </summary>
    [HttpPatch("{userId:guid}/activate")]
    public async Task<IActionResult> ActivateUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _userService.ActivateUserAsync(userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }
}