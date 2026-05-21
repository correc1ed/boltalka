using boltalka.Application.Enums.Call;

namespace boltalka.Application.Models.Call;

/// <summary>
/// Звонок (аудио или видео) в рамках чата.
/// </summary>
public class Call
{
    /// <summary>
    /// Уникальный идентификатор звонка.
    /// </summary>
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
    public CallStatus Status { get; set; }

    /// <summary>
    /// Тип звонка: аудио или видео.
    /// </summary>
    public CallType Type { get; set; }

    /// <summary>
    /// Навигационное свойство: чат, в котором звонок.
    /// </summary>
    public Chat.Chat Chat { get; set; } = null!;

    /// <summary>
    /// Навигационное свойство: инициатор звонка.
    /// </summary>
    public User.User Initiator { get; set; } = null!;
}