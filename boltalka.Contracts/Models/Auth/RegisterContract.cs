namespace boltalka.Contracts.Models.Auth;

public class RegisterContract
{
    /// <summary>
    /// Логин.
    /// </summary>
    public string Login { get; set; } = string.Empty;
    
    /// <summary>
    /// Пароль.
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Отображаемое имя.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}