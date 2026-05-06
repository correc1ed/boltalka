using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using boltalka.Infrastructure.Database.Enum;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Entities;

/// <summary>
/// Звонок (аудио или видео) в рамках чата.
/// </summary>
public class CallEntity
{
    /// <summary>
    /// Уникальный идентификатор звонка.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор чата, в котором совершён звонок.
    /// </summary>
    public Guid ChatId { get; set; }

    /// <summary>
    /// Идентификатор пользователя, инициировавшего звонок.
    /// </summary>
    public Guid InitiatorId { get; set; }

    /// <summary>
    /// Время начала звонка.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Время завершения звонка (null, если ещё не завершён).
    /// </summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>
    /// Текущий статус звонка (Pending, Active, Ended, Missed).
    /// </summary>
    [Required]
    public CallStatus Status { get; set; }

    /// <summary>
    /// Тип звонка: аудио или видео.
    /// </summary>
    [Required]
    public CallType Type { get; set; }

    /// <summary>
    /// Навигационное свойство: чат, в котором звонок.
    /// </summary>
    [ForeignKey(nameof(ChatId))]
    public ChatEntity ChatEntity { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство: инициатор звонка.
    /// </summary>
    [ForeignKey(nameof(InitiatorId))]
    public UserEntity Initiator { get; set; } = null!;
}