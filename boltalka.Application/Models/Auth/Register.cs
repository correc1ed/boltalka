namespace boltalka.Application.Models.Auth;

/// <summary>
/// Регистрация.
/// </summary>
public class Register
{    
    /// <summary>
    /// Логин.
    /// </summary>
    public string Login { get; init; } = string.Empty;
    
    /// <summary>
    /// Пароль.
    /// </summary>
    public string Password { get; init; } = string.Empty;
    
    /// <summary>
    /// Отображаемое имя.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;
}