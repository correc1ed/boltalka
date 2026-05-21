using boltalka.Application.Enums.ChatMember;

namespace boltalka.Application.Models.ChatMember;

/// <summary>
/// Связь "участник чата" (many-to-many между User и Chat).
/// </summary>
public class ChatMember
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
    public MemberRole Role { get; set; }

    /// <summary>
    /// Дата и время присоединения к чату.
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Навигационное свойство: пользователь.
    /// </summary>
    public User.User User { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство: чат.
    /// </summary>
    public Chat.Chat Chat { get; set; } = null!;
}