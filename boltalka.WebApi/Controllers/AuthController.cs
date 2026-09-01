using AutoMapper;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models.Auth;
using boltalka.Contracts.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace boltalka.WebApi.Controllers;

/// <summary>
/// Авторизация.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;
    
    public AuthController(IAuthService authService, IMapper mapper)
    {
        _authService = authService;
        _mapper = mapper;
    }

    /// <summary>
    /// Регистрация пользователя.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterContract registerContract, CancellationToken cancellationToken)
    {
        var register = _mapper.Map<Register>(registerContract);
        
        var result = await _authService.RegisterAsync(register, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<AuthTokensContract>(result.Value));
    }

    /// <summary>
    /// Авторизация пользователя.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthTokensContract>> LoginAsync([FromBody] CredentialsContract credentialsContract, CancellationToken cancellationToken)
    {
        var credentials = _mapper.Map<Credentials>(credentialsContract);
        
        var result = await _authService.LoginAsync(credentials, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<AuthTokensContract>(result.Value));
    }

    /// <summary>
    /// Обновить токен.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokensContract>> RefreshAsync([FromBody] string refreshToken, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(_mapper.Map<AuthTokensContract>(result.Value));
    }

    /// <summary>
    /// Выйти из аккаунта.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync([FromBody] string refreshToken, CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAsync(refreshToken, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok();
    }
}