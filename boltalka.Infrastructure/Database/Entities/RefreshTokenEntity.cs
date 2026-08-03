using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using boltalka.Infrastructure.Database.Abstract;

namespace boltalka.Infrastructure.Database.Entities;

/// <summary>
/// Токен обновления.
/// </summary>
public class RefreshTokenEntity : BaseEntity
{
    /// <summary>
    /// Refresh-токен (строка, которую мы выдаём клиенту).
    /// </summary>
    [Required]
    [MaxLength(500)]
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
    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; } = null!;
}