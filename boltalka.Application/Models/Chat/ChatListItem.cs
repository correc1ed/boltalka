using boltalka.Application.Enums.Chat;

namespace boltalka.Application.Models.Chat;

/// <summary>
/// Элемент списка чатов.
/// </summary>
public class ChatListItem
{
    /// <summary>
    /// Идентификатор чата.
    /// </summary>
    public Guid ChatId { get; set; }
    
    /// <summary>
    /// Название чата.
    /// </summary>
    public string? ChatName { get; set; }
    
    /// <summary>
    /// Тип чата.
    /// </summary>
    public ChatType Type { get; set; }
    
    /// <summary>
    /// Последнее сообщение.
    /// </summary>
    public Message.Message? LastMessage { get; set; }
    
    /// <summary>
    /// Количество непрочитанных сообщений.
    /// </summary>
    public int UnreadCount { get; set; }
}