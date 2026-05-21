using boltalka.Application.Enums.Call;
using boltalka.Application.Models.Call;

namespace boltalka.Application.Abstractions.Repositories;

public interface ICallRepository : IRepository<Call>
{
    /// <summary>
    /// Получить активный звонок.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <returns>Активный звонок.</returns>
    Task<Call?> GetActiveCallAsync(Guid chatId);
    
    /// <summary>
    /// Получить историю звонков.
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="skip">Пропуск элементов.</param>
    /// <param name="take">Выборка элементов.</param>
    /// <returns>Получение истории звонков.</returns>
    Task<IEnumerable<Call>> GetCallHistoryAsync(Guid chatId, int skip, int take);
    
    /// <summary>
    /// Обновить статус звонка.
    /// </summary>
    /// <param name="callId">Идентификатор звонка.</param>
    /// <param name="status">Статус звонка.</param>
    /// <param name="endedAt">Дата конца звонка.</param>
    /// <returns>Обновление статуса звонка.</returns>
    Task UpdateStatusAsync(Guid callId, CallStatus status, DateTime? endedAt = null);
}