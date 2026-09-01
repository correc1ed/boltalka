namespace boltalka.Application.Models.Auth;

/// <summary>
/// Токены авторизации.
/// </summary>
public class AuthTokens
{
    /// <summary>
    /// Токен доступа.
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;
    
    /// <summary>
    /// Обновить токен.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
    
    /// <summary>
    /// Истекает в.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}