using boltalka.Contracts.Models.Media;

namespace boltalka.Contracts.Models.Message;

/// <summary>
/// Связь many-to-many: сообщение может иметь несколько вложенных файлов.
/// </summary>
public class MessageMediaContract
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
    public MessageContract Message { get; set; } = null!;

    /// <summary>
    /// Медиа.
    /// </summary>
    public MediaContract Media { get; set; } = null!;
}