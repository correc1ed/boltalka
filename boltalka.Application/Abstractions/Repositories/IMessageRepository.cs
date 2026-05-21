using boltalka.Application.Enums.Message;
using boltalka.Application.Models.Message;

namespace boltalka.Application.Abstractions.Repositories;

public interface IMessageRepository : IRepository<Message>
{
    /// <summary>
    /// Получить сообщения по id чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="skip">Пропуск элементов.</param>
    /// <param name="take">Выборка элементов.</param>
    /// <param name="before">До/Перед.</param>
    /// <returns>Получение сообщений.</returns>
    Task<IEnumerable<Message>> GetMessagesAsync(Guid chatId, int skip, int take, DateTime? before = null);
    
    /// <summary>
    /// Получить последнее сообщение по id чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <returns>Получение последнего сообщения.</returns>
    Task<Message?> GetLastMessageAsync(Guid chatId);
    
    /// <summary>
    /// Обновить статус сообщения.
    /// </summary>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <param name="status">Статус сообщения.</param>
    /// <returns>Обновления статуса сообщения.</returns>
    Task UpdateStatusAsync(Guid messageId, MessageStatus status);
}