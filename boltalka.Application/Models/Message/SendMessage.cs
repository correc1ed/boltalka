namespace boltalka.Application.Models.Message;

/// <summary>
/// Отправленное сообщение.
/// </summary>
public class SendMessage
{
    /// <summary>
    /// Идентификатор чата.
    /// </summary>
    public Guid ChatId { get; set; }
    
    /// <summary>
    /// Текст сообщения.
    /// </summary>
    public string? Text { get; set; }
    
    /// <summary>
    /// Идентификаторы загруженных медиафайлов.
    /// </summary>
    public List<Guid> MediaIds { get; set; } = new();
}