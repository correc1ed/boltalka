namespace boltalka.Infrastructure.Database.Enum;

/// <summary>Статус звонка.</summary>
public enum CallStatus
{
    /// <summary>Ожидает ответа.</summary>
    Pending,
    /// <summary>Активный разговор.</summary>
    Active,
    /// <summary>Завершён.</summary>
    Ended,
    /// <summary>Пропущен.</summary>
    Missed
}
