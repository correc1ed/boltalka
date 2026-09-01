using boltalka.Contracts.Models.Message;

namespace boltalka.Contracts.Models.Chat;

/// <summary>
/// Элемент списка чатов.
/// </summary>
public class ChatListItemContract
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
    public MessageContract? LastMessage { get; set; }
    
    /// <summary>
    /// Количество непрочитанных сообщений.
    /// </summary>
    public int UnreadCount { get; set; }
}