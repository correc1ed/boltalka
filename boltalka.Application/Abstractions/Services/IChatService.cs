using boltalka.Application.Enums.ChatMember;
using boltalka.Application.Models;
using boltalka.Application.Models.Chat;

namespace boltalka.Application.Abstractions.Services;

/// <summary>
/// Сервис чатов.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Создать личный чат с другим пользователем. Если уже существует – вернуть существующий.
    /// </summary>
    /// <param name="creatorUserId">Идентификатор создателя чата.</param>
    /// <param name="otherUserId">Идентификатор другого пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Информация о созданном чате.</returns>
    Task<Result<ChatInfo>> CreatePrivateChatAsync(Guid creatorUserId, Guid otherUserId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Создать групповой чат.
    /// </summary>
    /// <param name="creatorUserId">Идентификатор создателя чата.</param>
    /// <param name="name">Название чата.</param>
    /// <param name="memberIds">Идентификаторы других участников чата.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Информация о созданном чате.</returns>
    Task<Result<ChatInfo>> CreateGroupChatAsync(Guid creatorUserId, string name, IEnumerable<Guid> memberIds, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить информацию о чате (с участниками).
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="requestUserId">Идентификатор пользователя, который отправил запрос.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Информация о чате.</returns>
    Task<Result<ChatInfo>> GetChatAsync(Guid chatId, Guid requestUserId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить список чатов пользователя с последним сообщением (пагинация).
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="skip">Пропуск элементов.</param>
    /// <param name="take">Выборка элементов.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Список чатов пользователя.</returns>
    Task<Result<IEnumerable<ChatListItem>>> GetUserChatsAsync(Guid userId, int skip, int take, CancellationToken cancellationToken);
    
    /// <summary>
    /// Добавить участника в групповой чат (админ).
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="newMemberId">Идентификатор нового пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Добавление нового пользователя в чат.</returns>
    Task<Result> AddMemberAsync(Guid chatId, Guid userId, Guid newMemberId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Удалить участника из группового чата (админ или выход сам).
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="targetUserId">Идентификатор удаляемого пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Удаление участника из группового чата.</returns>
    Task<Result> RemoveMemberAsync(Guid chatId, Guid userId, Guid targetUserId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Изменить роль участника (Member ↔ Admin).
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="targetUserId">Идентификатор пользователя, роль которого необходимо поменять.</param>
    /// <param name="newRole">Новая роль.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Изменение роли честника.</returns>
    Task<Result> ChangeMemberRoleAsync(Guid chatId, Guid userId, Guid targetUserId, MemberRole newRole, CancellationToken cancellationToken);
    
    /// <summary>
    /// Изменить название группового чата.
    /// </summary>
    /// <param name="chatId">Идентификатор чата.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="newName">Новое название.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Изменение названия группового чата.</returns>
    Task<Result> UpdateChatNameAsync(Guid chatId, Guid userId, string newName, CancellationToken cancellationToken);
}