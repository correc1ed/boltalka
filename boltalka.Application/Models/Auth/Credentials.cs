namespace boltalka.Application.Models.Auth;

/// <summary>
/// Логин и пароль.
/// </summary>
public class Credentials
{
    /// <summary>
    /// Логин.
    /// </summary>
    public string Login { get; init; } = string.Empty;
    
    /// <summary>
    /// Пароль.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}