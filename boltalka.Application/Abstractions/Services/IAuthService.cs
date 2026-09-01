using boltalka.Application.Models;
using boltalka.Application.Models.Auth;

namespace boltalka.Application.Abstractions.Services;

/// <summary>
/// Сервис авторизации.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Регистрация нового пользователя. Возвращает результат с созданным пользователем или ошибкой.
    /// </summary>
    /// <param name="register">Регистрация.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Токены авторизации.</returns>
    Task<Result<AuthTokens>> RegisterAsync(Register register, CancellationToken cancellationToken);
    
    /// <summary>
    /// Вход по логину и паролю. Возвращает пару токенов (access + refresh) или ошибку.
    /// </summary>
    /// <param name="credentials">Логин и пароль.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Токены авторизации.</returns>
    Task<Result<AuthTokens>> LoginAsync(Credentials credentials, CancellationToken cancellationToken);
    
    /// <summary>
    /// Обновление access-токена по действующему refresh-токену.
    /// </summary>
    /// <param name="refreshToken">Refresh токен.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Токены авторизации.</returns>
    Task<Result<AuthTokens>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    
    /// <summary>
    /// Выход (аннулирование refresh-токена).
    /// </summary>
    /// <param name="refreshToken">Refresh токен.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Выход.</returns>
    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}