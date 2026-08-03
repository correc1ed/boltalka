using boltalka.Application.Enums.Call;
using boltalka.Application.Models.Call;

namespace boltalka.Application.Abstractions.Repositories;

public interface ICallRepository : IRepository<Call>
{
    /// <summary>
    /// Получить активный звонок.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Активный звонок.</returns>
    Task<Call?> GetActiveCallAsync(Guid chatId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить историю звонков.
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="skip">Пропуск элементов.</param>
    /// <param name="take">Выборка элементов.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Получение истории звонков.</returns>
    Task<IEnumerable<Call>> GetCallHistoryAsync(Guid chatId, int skip, int take, CancellationToken cancellationToken);
    
    /// <summary>
    /// Обновить статус звонка.
    /// </summary>
    /// <param name="callId">Идентификатор звонка.</param>
    /// <param name="status">Статус звонка.</param>
    /// <param name="endedAt">Дата конца звонка.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Обновление статуса звонка.</returns>
    Task UpdateStatusAsync(Guid callId, CallStatus status, CancellationToken cancellationToken, DateTime? endedAt = null);
}