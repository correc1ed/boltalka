using boltalka.Application.Models.Chat;

namespace boltalka.Application.Abstractions.Repositories;

public interface IChatRepository : IRepository<Chat>
{
    /// <summary>
    /// Получить данные чата по id.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <returns>Получение сущности чата.</returns>
    Task<Chat?> GetChatWithMembersAsync(Guid chatId);
    
    /// <summary>
    /// Получить чаты пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="skip">Пропуск элементов.</param>
    /// <param name="take">Выборка элементов.</param>
    /// <returns>Получение чатов пользователя.</returns>
    Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId, int skip, int take);
    
    /// <summary>
    /// Признак того, что пользователь находится в чате.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>Пользователь находится в чате.</returns>
    Task<bool> IsUserInChatAsync(Guid chatId, Guid userId);
}