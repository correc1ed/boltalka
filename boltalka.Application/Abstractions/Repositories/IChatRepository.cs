using boltalka.Application.Enums.ChatMember;
using boltalka.Application.Models.Chat;

namespace boltalka.Application.Abstractions.Repositories;

public interface IChatRepository : IRepository<Chat>
{
    /// <summary>
    /// Получить данные чата по id.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Получение сущности чата.</returns>
    Task<Chat?> GetChatWithMembersAsync(Guid chatId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить чаты пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="skip">Пропуск элементов.</param>
    /// <param name="take">Выборка элементов.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Получение чатов пользователя.</returns>
    Task<IEnumerable<Chat>> GetUserChatsAsync(Guid userId, int skip, int take, CancellationToken cancellationToken);
    
    /// <summary>
    /// Признак того, что пользователь находится в чате.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Пользователь находится в чате.</returns>
    Task<bool> IsUserInChatAsync(Guid chatId, Guid userId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить роль участника в чате.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Роль.</returns>
    Task<MemberRole?> GetMemberRoleAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Добавление роли пользователя в чате.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="role">Роль.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Добавление роли пользователя в чат.</returns>
    Task AddMemberToChatAsync(Guid chatId, Guid userId, MemberRole role, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновление роли пользователя в чате.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="newRole">Новая роль.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Обновление роли пользователя в чате.</returns>
    Task UpdateMemberRoleAsync(Guid chatId, Guid userId, MemberRole newRole, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаление роли пользователя в чате.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Удаление роли пользователя в чате.</returns>
    Task RemoveMemberFromChatAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default);
}