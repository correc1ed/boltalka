namespace boltalka.Application.Models.MessageMedia;

/// <summary>
/// Связь many-to-many: сообщение может иметь несколько вложенных файлов.
/// </summary>
public class MessageMedia
{
    /// <summary>
    /// Идентификатор сообщения.
    /// </summary>
    public Guid MessageId { get; set; }
    
    /// <summary>
    /// Идентификатор медиа.
    /// </summary>
    public Guid MediaId { get; set; }

    /// <summary>Порядок отображения (первое, второе…).</summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Сообщение.
    /// </summary>
    public Message.Message Message { get; set; } = null!;

    /// <summary>
    /// Медиа.
    /// </summary>
    public Media.Media Media { get; set; } = null!;
}