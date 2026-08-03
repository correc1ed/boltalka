using boltalka.Application.Enums.Chat;
using boltalka.Application.Models.User;

namespace boltalka.Application.Models.Chat;

/// <summary>
/// Информация о чате.
/// </summary>
public class ChatInfo
{
    /// <summary>
    /// Идентификатор чата.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Название чата.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Тип чата.
    /// </summary>
    public ChatType Type { get; set; }
    
    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Участники чата.
    /// </summary>
    public ICollection<UserProfile> Members { get; set; } = new List<UserProfile>();
    
    /// <summary>
    /// Последнее сообщение.
    /// </summary>
    public Message.Message? LastMessage { get; set; }
}