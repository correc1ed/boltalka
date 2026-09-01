namespace boltalka.Contracts.Models.Auth;

public class AuthTokensContract
{
    /// <summary>
    /// Токен доступа.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Обновить токен.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Истекает в.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}