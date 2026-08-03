using boltalka.Application.Enums.Call;
using boltalka.Application.Models;
using boltalka.Application.Models.Call;

namespace boltalka.Application.Abstractions.Services;

/// <summary>
/// Сервис звонков.
/// </summary>
public interface ICallService
{
    /// <summary>
    /// Начать звонок в чате.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="initiatorId">Идентификатор инициатора.</param>
    /// <param name="callType">Тип звонка.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Информация о звонке.</returns>
    Task<Result<Call>> StartCallAsync(Guid chatId, Guid initiatorId, CallType callType, CancellationToken cancellationToken);
    
    /// <summary>
    /// Принять звонок (участником).
    /// </summary>
    /// <param name="callId">Идентификатор звонка.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Информация о звонке.</returns>
    Task<Result<Call>> AcceptCallAsync(Guid callId, Guid userId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Завершить звонок.
    /// </summary>
    /// <param name="callId">Идентификатор звонка.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Информация о звонке.</returns>
    Task<Result<Call>> EndCallAsync(Guid callId, Guid userId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Отклонить звонок (или пропустить).
    /// </summary>
    /// <param name="callId">Идентификатор звонка.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Отклонение звонка.</returns>
    Task<Result> DeclineCallAsync(Guid callId, Guid userId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить информацию о текущем активном звонке в чате.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Информация о текущем активном звонке.</returns>
    Task<Result<Call?>> GetActiveCallAsync(Guid chatId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить историю звонков чата (пагинация).
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="skip">Пропуск элементов.</param>
    /// <param name="take">Выборка элементов.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Список звонков чата.</returns>
    Task<Result<IEnumerable<Call>>> GetCallHistoryAsync(Guid chatId, Guid userId, int skip, int take, CancellationToken cancellationToken);
}