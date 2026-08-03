namespace boltalka.Application.Abstractions.Repositories;

public interface IRepository<T> where T : class
{
    /// <summary>
    /// Получить запись по id.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Получение записи по id.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить все записи.
    /// </summary>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Получение всех записей.</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Добавить запись.
    /// </summary>
    /// <param name="model">Запись.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Добавление записи.</returns>
    Task AddAsync(T model, CancellationToken cancellationToken);
    
    /// <summary>
    /// Обновить запись.
    /// </summary>
    /// <param name="model">Запись.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    Task UpdateAsync(T model, CancellationToken cancellationToken);
    
    /// <summary>
    /// Удалить запись.
    /// </summary>
    /// <param name="model">Запись.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    Task DeleteAsync(T model, CancellationToken cancellationToken);
}