namespace boltalka.Contracts.Models.Auth;

public class CredentialsContract
{
    /// <summary>
    /// Логин.
    /// </summary>
    public string Login { get; set; } = string.Empty;
    
    /// <summary>
    /// Парлоь.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}