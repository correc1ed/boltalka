using boltalka.Application.Enums.Message;

namespace boltalka.Application.Models.Message;

/// <summary>
/// Информация о сообщении.
/// </summary>
public class MessageInfo
{
    /// <summary>
    /// Идентификатор сообщения.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Идентификатор чата.
    /// </summary>
    public Guid ChatId { get; set; }
    
    /// <summary>
    /// Идентификатор отправителя.
    /// </summary>
    public Guid SenderId { get; set; }
    
    /// <summary>
    /// Наименование отправителя.
    /// </summary>
    public string SenderName { get; set; } = string.Empty;
    
    /// <summary>
    /// Текст сообщения.
    /// </summary>
    public string? Text { get; set; }
    
    /// <summary>
    /// Список вложений.
    /// </summary>
    public IEnumerable<Media.Media> Attachments { get; set; } = new List<Media.Media>();
    
    /// <summary>
    /// Статус сообщения.
    /// </summary>
    public MessageStatus Status { get; set; }
    
    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата обновления.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}