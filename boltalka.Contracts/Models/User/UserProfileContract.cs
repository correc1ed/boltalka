namespace boltalka.Contracts.Models.User;

/// <summary>
/// Профиль пользователя.
/// </summary>
public class UserProfileContract
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Логин.
    /// </summary>
    public string Login { get; set; } = string.Empty;
    
    /// <summary>
    /// Отображаемое имя.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Ссылка на аватар пользователя.
    /// </summary>
    public string? AvatarUrl { get; set; }   // Можно формировать из Media.StoragePath.
    
    /// <summary>
    /// Признак активности пользователя.
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата обновления.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}