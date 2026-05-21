using boltalka.Application.Enums.Message;

namespace boltalka.Application.Models.Message;

/// <summary>
/// Сообщение в чате.
/// </summary>
public class Message
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата обновления.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Идентификатор чата, в котором отправлено сообщение.
    /// </summary>
    public Guid ChatId { get; set; }

    /// <summary>
    /// Идентификатор отправителя.
    /// </summary>
    public Guid SenderId { get; set; }

    /// <summary>
    /// Текст сообщения (может быть null, если только вложение).
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// URL прикреплённого файла (изображение, документ).
    /// </summary>
    public string? AttachmentUrl { get; set; }

    /// <summary>
    /// Статус доставки: Sent, Delivered, Read.
    /// </summary>
    public MessageStatus Status { get; set; }

    /// <summary>
    /// Навигационное свойство: чат, к которому относится сообщение.
    /// </summary>
    public Chat.Chat Chat { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство: отправитель.
    /// </summary>
    public User.User Sender { get; set; } = null!;
    
    /// <summary>
    /// Список отправдляемых медиа в сообщении.
    /// </summary>
    public ICollection<MessageMedia.MessageMedia> MessageMediaLinks { get; set; } = new List<MessageMedia.MessageMedia>();
}