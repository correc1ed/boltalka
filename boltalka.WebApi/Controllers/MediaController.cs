using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using boltalka.Application.Abstractions.Services;
using boltalka.Contracts.Models.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace boltalka.WebApi.Controllers;

/// <summary>
/// Управление медиафайлами.
/// </summary>
[ApiController]
[Route("api/v1/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly IMapper _mapper;

    public MediaController(IMediaService mediaService, IMapper mapper)
    {
        _mediaService = mediaService;
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
    /// Загрузить файл.
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Файл не предоставлен или пуст." });

        var uploaderUserId = GetCurrentUserId();
        await using var stream = file.OpenReadStream();

        var result = await _mediaService.UploadAsync(
            uploaderUserId,
            stream,
            file.FileName,
            file.ContentType,
            cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<MediaContract>(result.Value));
    }

    /// <summary>
    /// Получить информацию о медиафайле.
    /// </summary>
    [HttpGet("{mediaId:guid}")]
    public async Task<ActionResult<MediaContract>> GetMediaInfoAsync(Guid mediaId, CancellationToken cancellationToken)
    {
        var result = await _mediaService.GetMediaAsync(mediaId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<MediaContract>(result.Value));
    }

    /// <summary>
    /// Скачать медиафайл.
    /// </summary>
    [HttpGet("{mediaId:guid}/download")]
    public async Task<IActionResult> DownloadAsync(Guid mediaId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var mediaResult = await _mediaService.GetMediaAsync(mediaId, cancellationToken);
        if (!mediaResult.IsSuccess)
            return BadRequest(new { error = mediaResult.Error });

        var streamResult = await _mediaService.GetFileStreamAsync(mediaId, userId, cancellationToken);
        if (!streamResult.IsSuccess)
            return BadRequest(new { error = streamResult.Error });

        var media = mediaResult.Value!;
        
        if (mediaResult.Value is null || streamResult.Value is null)
            return NotFound();
        
        return File(streamResult.Value, media.ContentType, media.FileName);
    }

    /// <summary>
    /// Удалить медиафайл.
    /// </summary>
    [HttpDelete("{mediaId:guid}")]
    public async Task<IActionResult> DeleteMediaAsync(Guid mediaId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _mediaService.DeleteMediaAsync(mediaId, userId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }
}