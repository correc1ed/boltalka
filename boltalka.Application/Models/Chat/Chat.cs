using boltalka.Application.Enums.Chat;

namespace boltalka.Application.Models.Chat;

/// <summary>
/// Чат (личный или групповой).
/// </summary>
public class Chat
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public Guid Id { get; init; }
    
    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Дата обновления.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
    
    /// <summary>
    /// Название чата (для групповых; у личных может быть null или пустым).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Тип чата: Private (1 на 1) или Group.
    /// </summary>
    public ChatType Type { get; init; }

    /// <summary>
    /// Участники чата.
    /// </summary>
    public ICollection<ChatMember.ChatMember> Members { get; set; } = new List<ChatMember.ChatMember>();

    /// <summary>
    /// Сообщения в чате.
    /// </summary>
    public ICollection<Message.Message> Messages { get; init; } = new List<Message.Message>();

    /// <summary>
    /// Звонки, совершённые в этом чате.
    /// </summary>
    public ICollection<Call.Call> Calls { get; init; } = new List<Call.Call>();
}