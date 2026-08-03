using boltalka.Application.Models;
using boltalka.Application.Models.Media;

namespace boltalka.Application.Abstractions.Services;

/// <summary>
/// Сервис медиафайлов.
/// </summary>
public interface IMediaService
{
    /// <summary>
    /// Загрузить файл (аватар, вложение).
    /// </summary>
    /// <param name="uploaderUserId">Идентификатор пользователя, загружающего файл.</param>
    /// <param name="content">Содержимое.</param>
    /// <param name="fileName">Название файла.</param>
    /// <param name="contentType">Тип файла.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Данные о медиа.</returns>
    Task<Result<Media>> UploadAsync(Guid uploaderUserId, Stream content, string fileName, string contentType, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить метаданные по ID.
    /// </summary>
    /// <param name="mediaId">Идентификатор медиафайла.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Метаданные файла.</returns>
    Task<Result<Media>> GetMediaAsync(Guid mediaId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Получить поток для скачивания (с проверкой прав доступа).
    /// </summary>
    /// <param name="mediaId">Идентификатор медиафайла.</param>
    /// <param name="requestUserId">Идентификатор пользователя, который отправил запрос.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Поток для скачивания.</returns>
    Task<Result<Stream>> GetFileStreamAsync(Guid mediaId, Guid requestUserId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Удалить медиа (если не используется).
    /// </summary>
    /// <param name="mediaId">Идентификатор медиафайла.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Удаление медиафайла.</returns>
    Task<Result> DeleteMediaAsync(Guid mediaId, Guid userId, CancellationToken cancellationToken);
}