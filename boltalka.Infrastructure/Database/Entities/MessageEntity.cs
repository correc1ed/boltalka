using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using boltalka.Infrastructure.Database.Abstract;
using boltalka.Infrastructure.Database.Enum;

namespace boltalka.Infrastructure.Database.Entities;

/// <summary>
/// Сообщение в чате.
/// </summary>
public class MessageEntity : BaseEntity
{
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
    [MaxLength(5000)]
    public string? Text { get; set; }

    /// <summary>
    /// URL прикреплённого файла (изображение, документ).
    /// </summary>
    [MaxLength(500)]
    [Url]
    public string? AttachmentUrl { get; set; }

    /// <summary>
    /// Статус доставки: Sent, Delivered, Read.
    /// </summary>
    [Required]
    public MessageStatus Status { get; set; }

    /// <summary>
    /// Навигационное свойство: чат, к которому относится сообщение.
    /// </summary>
    [ForeignKey(nameof(ChatId))]
    public ChatEntity ChatEntity { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство: отправитель.
    /// </summary>
    [ForeignKey(nameof(SenderId))]
    public UserEntity Sender { get; set; } = null!;
    
    /// <summary>
    /// Список отправдляемых медиа в сообщении.
    /// </summary>
    public ICollection<MessageMediaEntity> MessageMediaLinks { get; set; } = new List<MessageMediaEntity>();
}