namespace boltalka.Application.Abstractions.Repositories;

public interface IRepository<T> where T : class
{
    /// <summary>
    /// Получить запись по id.
    /// </summary>
    /// <param name="id">Идентификатор.</param>
    /// <returns>Получение записи по id.</returns>
    Task<T?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Получить все записи.
    /// </summary>
    /// <returns>Получение всех записей.</returns>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Добавить запись.
    /// </summary>
    /// <param name="entity">Запись.</param>
    /// <returns>Добавление записи.</returns>
    Task AddAsync(T entity);
    
    /// <summary>
    /// Обновить запись.
    /// </summary>
    /// <param name="entity">Запись.</param>
    void Update(T entity);
    
    /// <summary>
    /// Удалить запись.
    /// </summary>
    /// <param name="entity">Запись.</param>
    void Delete(T entity);
}