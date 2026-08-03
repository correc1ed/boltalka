using boltalka.Application.Models.Call;
using boltalka.Application.Models.Message;

namespace boltalka.Application.Abstractions.Services;

/// <summary>
/// Сервис уведомлений.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Отправить уведомление о новом сообщении участникам чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="message">Сообщение.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <param name="excludeUserIds">Список идентификаторов пользователей, которым не нужно отправлять уведомление.</param>
    /// <returns>Отправка уведомления о новом сообщении участникам чата.</returns>
    Task NotifyNewMessageAsync(Guid chatId, Message message, CancellationToken cancellationToken, IEnumerable<Guid>? excludeUserIds = null);
    
    /// <summary>
    /// Отправить уведомление о входящем звонке.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="call">Звонок.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <param name="excludeUserIds">Список идентификаторов пользователей, которым не нужно отправлять уведомление.</param>
    /// <returns>Отправка уведомления о входящем звонке.</returns>
    Task NotifyIncomingCallAsync(Guid chatId, Call call, CancellationToken cancellationToken, IEnumerable<Guid>? excludeUserIds = null);
    
    /// <summary>
    /// Отправить уведомление об изменении статуса звонка.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="call">Звонок.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Отправка уведомления об изменении статуса звонка.</returns>
    Task NotifyCallStatusChangedAsync(Guid chatId, Call call, CancellationToken cancellationToken);
    
    /// <summary>
    /// Отправить уведомление о новом участнике чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="newUserId">Идентификатор нового участника чата.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Отправка уведомления о новом участнике чата.</returns>
    Task NotifyMemberAddedAsync(Guid chatId, Guid newUserId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Отправить уведомление о выходе/удалении участника.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="removedUserId">Идентификатор удаляемого пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Отправка уведомления о выходе/удалении участника.</returns>
    Task NotifyMemberRemovedAsync(Guid chatId, Guid removedUserId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Отправить системное сообщение в чат (например, изменение названия).
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="text">Текст системного сообщения.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Отправка системного сообщения в чат (например, изменение названия).</returns>
    Task NotifySystemMessageAsync(Guid chatId, string text, CancellationToken cancellationToken);
}