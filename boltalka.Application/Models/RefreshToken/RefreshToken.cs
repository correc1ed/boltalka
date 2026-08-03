namespace boltalka.Application.Models.RefreshToken;

/// <summary>
/// Токен обновления.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата обновления.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Refresh-токен (строка, которую мы выдаём клиенту).
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата и время истечения срока действия.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя, которому принадлежит токен.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Навигационное свойство.
    /// </summary>
    public User.User User { get; set; } = null!;
}