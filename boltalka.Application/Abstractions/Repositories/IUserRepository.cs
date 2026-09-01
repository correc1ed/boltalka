using boltalka.Application.Models.User;

namespace boltalka.Application.Abstractions.Repositories;

public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Получить пользователя по логину.
    /// </summary>
    /// <param name="login">Логин.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Получение пользователя по логину.</returns>
    Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить полную информацию о пользователе по id.
    /// </summary>
    /// <param name="id">дентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Получение полной информации о пользователе.</returns>
    Task<User?> GetFullInfoByIdAsync(Guid id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Признак того, что пользователь по указанному логину существует.
    /// </summary>
    /// <param name="login">Логин.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Пользователь с таким логином существует.</returns>
    Task<bool> LoginExistsAsync(string login, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить пользователей.
    /// </summary>
    /// <param name="skip">Пропуск элементов.</param>
    /// <param name="take">Выборка элементов.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Получение пользователей.</returns>
    Task<IEnumerable<User>> GetUsersAsync(int skip, int take, CancellationToken cancellationToken);
    
    /// <summary>
    /// Поиск пользователей.
    /// </summary>
    /// <param name="query">Имя пользователя.</param>
    /// <param name="skip">Пропуск элементов.</param>
    /// <param name="take">Выборка элементов.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Список пользователей.</returns>
    Task<IEnumerable<User>> SearchUsersAsync(string query, int skip, int take, CancellationToken cancellationToken = default);
}