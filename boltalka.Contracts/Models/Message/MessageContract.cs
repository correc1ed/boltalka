using boltalka.Contracts.Models.Chat;
using boltalka.Contracts.Models.User;

namespace boltalka.Contracts.Models.Message;

/// <summary>
/// Сообщение в чате.
/// </summary>
public class MessageContract
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
    public MessageStatusContract Status { get; set; }

    /// <summary>
    /// Навигационное свойство: чат, к которому относится сообщение.
    /// </summary>
    public ChatContract Chat { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство: отправитель.
    /// </summary>
    public UserContract Sender { get; set; } = null!;
    
    /// <summary>
    /// Список отправдляемых медиа в сообщении.
    /// </summary>
    public ICollection<MessageMediaContract> MessageMediaLinks { get; set; } = new List<MessageMediaContract>();
}