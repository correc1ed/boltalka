using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using boltalka.Infrastructure.Database.Abstract;

namespace boltalka.Infrastructure.Database.Entities;

/// <summary>
/// Файл, загруженный в мессенджер (аватар, вложение сообщения).
/// </summary>
public class MediaEntity : BaseEntity
{
    /// <summary>Оригинальное имя файла (для скачивания).</summary>
    [MaxLength(256)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>MIME-тип (image/png, application/pdf и т.п.).</summary>
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Размер в байтах.</summary>
    public long SizeBytes { get; set; }

    /// <summary>Путь в хранилище или URL (относительный или абсолютный).</summary>
    [MaxLength(500)]
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>Пользователь, загрузивший файл.</summary>
    public Guid? UploadedByUserId { get; set; }

    // Навигационные свойства
    public UserEntity? UploadedBy { get; set; }
}