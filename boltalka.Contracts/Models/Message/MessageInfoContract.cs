using boltalka.Contracts.Models.Media;

namespace boltalka.Contracts.Models.Message;

public class MessageInfoContract
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
    public IEnumerable<MediaContract> Attachments { get; set; } = new List<MediaContract>();
    
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