using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using boltalka.Infrastructure.Database.Enum;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Entities;

/// <summary>
/// Связь "участник чата" (many-to-many между User и Chat).
/// </summary>
[PrimaryKey(nameof(UserId), nameof(ChatId))]
public class ChatMemberEntity
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Идентификатор чата.
    /// </summary>
    public Guid ChatId { get; set; }

    /// <summary>
    /// Роль участника в чате (Member или Admin).
    /// </summary>
    [Required]
    public MemberRole Role { get; set; }

    /// <summary>
    /// Дата и время присоединения к чату.
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Навигационное свойство: пользователь.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public UserEntity UserEntity { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство: чат.
    /// </summary>
    [ForeignKey(nameof(ChatId))]
    public ChatEntity ChatEntity { get; set; } = null!;
}