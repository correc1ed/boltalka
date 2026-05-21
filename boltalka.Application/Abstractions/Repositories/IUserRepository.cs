using boltalka.Application.Models.User;

namespace boltalka.Application.Abstractions.Repositories;

public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Получить пользователя по логину.
    /// </summary>
    /// <param name="login">Логин.</param>
    /// <returns>Получение пользователя по логину.</returns>
    Task<User?> GetByLoginAsync(string login);
    
    /// <summary>
    /// Признак того, что пользователь по указанному логину существует.
    /// </summary>
    /// <param name="login">Логин.</param>
    /// <returns>Пользователь с таким логином существует.</returns>
    Task<bool> LoginExistsAsync(string login);
    
    /// <summary>
    /// Получить пользователей.
    /// </summary>
    /// <param name="skip">Пропуск элементов.</param>
    /// <param name="take">Выборка элементов.</param>
    /// <returns>Получение пользователей.</returns>
    Task<IEnumerable<User>> GetUsersAsync(int skip, int take);
}