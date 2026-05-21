namespace boltalka.Application.Models.Media;

/// <summary>
/// Файл, загруженный в мессенджер (аватар, вложение сообщения).
/// </summary>
public class Media
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата обновления.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>Оригинальное имя файла (для скачивания).</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>MIME-тип (image/png, application/pdf и т.п.).</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Размер в байтах.</summary>
    public long SizeBytes { get; set; }

    /// <summary>Путь в хранилище или URL (относительный или абсолютный).</summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>Пользователь, загрузивший файл.</summary>
    public Guid? UploadedByUserId { get; set; }

    // Навигационные свойства
    public User.User? UploadedBy { get; set; }
}