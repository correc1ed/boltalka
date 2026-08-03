using boltalka.Application.Models;
using boltalka.Application.Models.Message;

namespace boltalka.Application.Abstractions.Services;

/// <summary>
/// Сервис сообщений.
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Отправить сообщение (текст и/или вложения).
    /// </summary>
    /// <param name="senderId">Идентификатор отправителя.</param>
    /// <param name="sendMessage">Отправленное сообщение.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Информация о сообщении.</returns>
    Task<Result<MessageInfo>> SendMessageAsync(Guid senderId, SendMessage sendMessage, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить сообщения чата (пагинация, курсорная по дате).
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="skip">Пропуск элементов.</param>
    /// <param name="take">Выборка элементов.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <param name="before">До.</param>
    /// <returns>Список сообщений из чата.</returns>
    Task<Result<IEnumerable<Message>>> GetMessagesAsync(Guid chatId, Guid userId, int skip, int take, CancellationToken cancellationToken, DateTime? before = null);
    
    /// <summary>
    /// Пометить сообщения как прочитанные (для личных чатов или группы – сложнее, пока для 1-на-1).
    /// </summary>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Помечение сообщения как прочитанное.</returns>
    Task<Result> MarkAsReadAsync(Guid messageId, Guid userId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Редактировать текст сообщения (только своё, в течение ограниченного времени).
    /// </summary>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="newText">Новый текст.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Информация о сообщении.</returns>
    Task<Result<MessageInfo>> EditMessageAsync(Guid messageId, Guid userId, string newText, CancellationToken cancellationToken);
    
    /// <summary>
    /// Мягкое удаление сообщения (только своё).
    /// </summary>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Информация о сообщении.</returns>
    Task<Result> DeleteMessageAsync(Guid messageId, Guid userId, CancellationToken cancellationToken);
}