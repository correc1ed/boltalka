using boltalka.Contracts.Models.Message;
using boltalka.Contracts.Models.User;

namespace boltalka.Contracts.Models.Chat;

/// <summary>
/// Информация о чате.
/// </summary>
public class ChatInfoContract
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
    public ICollection<UserProfileContract> Members { get; set; } = new List<UserProfileContract>();
    
    /// <summary>
    /// Последнее сообщение.
    /// </summary>
    public MessageContract? LastMessage { get; set; }
}