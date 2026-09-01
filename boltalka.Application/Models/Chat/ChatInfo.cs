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
    public Guid Id { get; init; }
    
    /// <summary>
    /// Название чата.
    /// </summary>
    public string? Name { get; init; }
    
    /// <summary>
    /// Тип чата.
    /// </summary>
    public ChatType Type { get; init; }
    
    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Участники чата.
    /// </summary>
    public ICollection<UserProfile> Members { get; init; } = new List<UserProfile>();
    
    /// <summary>
    /// Последнее сообщение.
    /// </summary>
    public Message.Message? LastMessage { get; init; }
}