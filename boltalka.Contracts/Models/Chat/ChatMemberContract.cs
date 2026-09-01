using boltalka.Contracts.Models.User;

namespace boltalka.Contracts.Models.Chat;

/// <summary>
/// Связь "участник чата" (many-to-many между User и Chat).
/// </summary>
public class ChatMemberContract
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
    public UserContract User { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство: чат.
    /// </summary>
    public ChatContract Chat { get; set; } = null!;
}