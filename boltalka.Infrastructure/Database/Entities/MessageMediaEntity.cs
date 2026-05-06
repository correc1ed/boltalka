using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Entities;

/// <summary>
/// Связь many-to-many: сообщение может иметь несколько вложенных файлов.
/// </summary>
[PrimaryKey(nameof(MessageId), nameof(MediaId))]
public class MessageMediaEntity
{
    public Guid MessageId { get; set; }
    public Guid MediaId { get; set; }

    /// <summary>Порядок отображения (первое, второе…).</summary>
    public int SortOrder { get; set; }

    [ForeignKey(nameof(MessageId))]
    public MessageEntity MessageEntity { get; set; } = null!;

    [ForeignKey(nameof(MediaId))]
    public MediaEntity MediaEntity { get; set; } = null!;
}