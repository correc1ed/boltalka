using boltalka.Application.Models;
using boltalka.Application.Models.User;

namespace boltalka.Application.Abstractions.Services;

/// <summary>
/// Сервис пользователя.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Получение профиля текущего пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Профиль пользователя.</returns>
    Task<Result<UserProfile>> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Обновление отображаемого имени.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="newDisplayName">Новое отображаемое имя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Профиль пользователя.</returns>
    Task<Result<UserProfile>> UpdateDisplayNameAsync(Guid userId, string newDisplayName, CancellationToken cancellationToken);
    
    /// <summary>
    /// Установка/смена аватара (по ID уже загруженного медиафайла).
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="mediaId">Идентификатор медиа.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Профиль пользователя.</returns>
    Task<Result<UserProfile>> SetAvatarAsync(Guid userId, Guid mediaId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Поиск пользователей по логину или имени (пагинация).
    /// </summary>
    /// <param name="query">Запрос.</param>
    /// <param name="skip">Пропуск элементов.</param>
    /// <param name="take">Выборка элементов.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Список профилей пользователей.</returns>
    Task<Result<IEnumerable<UserProfile>>> SearchUsersAsync(string query, int skip, int take, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получение публичной информации о пользователе по ID.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Профиль пользователя.</returns>
    Task<Result<UserProfile>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Деактивировать пользователя (IsActive = false).
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Деактивация пользователя.</returns>
    Task<Result> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Активировать пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Активация пользователя.</returns>
    Task<Result> ActivateUserAsync(Guid userId, CancellationToken cancellationToken);
}