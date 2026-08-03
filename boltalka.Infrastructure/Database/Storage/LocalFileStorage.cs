using boltalka.Application.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace boltalka.Infrastructure.Database.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;

    public LocalFileStorage(IOptions<FileStorageOptions> options)
    {
        _basePath = Path.GetFullPath(options.Value.BasePath);
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }
    
    public async Task<string> SaveAsync(string fileName, Stream content, string contentType, CancellationToken ct = default)
    {
        if (content == null || content.Length == 0)
            throw new ArgumentException("Файл пуст", nameof(content));

        // Генерируем уникальное имя, чтобы избежать конфликтов
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant() ?? "";
        var uniqueName = $"{Guid.NewGuid()}{ext}";

        // Группируем по датам для удобства
        var dateFolder = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var targetFolder = Path.Combine(_basePath, dateFolder);
        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        var fullPath = Path.Combine(targetFolder, uniqueName);
        // Дополнительная проверка, что путь не вышел за _basePath (защита от path traversal)
        if (!IsPathSafe(fullPath))
            throw new UnauthorizedAccessException("Недопустимый путь к файлу");

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        await content.CopyToAsync(fileStream, ct);

        // Возвращаем относительный путь (часть после _basePath)
        var relativePath = Path.GetRelativePath(_basePath, fullPath).Replace("\\", "/");
        return relativePath;
    }

    public async Task<Stream> OpenReadAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = GetSafeFullPath(storagePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Файл не найден", fullPath);

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        
        return stream;
    }

    public async Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = GetSafeFullPath(storagePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
    
    private string GetSafeFullPath(string relativePath)
    {
        // Защита: не позволяем выйти за пределы _basePath
        var fullPath = Path.GetFullPath(Path.Combine(_basePath, relativePath));
        
        if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Недопустимый путь к файлу");
        
        return fullPath;
    }

    private bool IsPathSafe(string fullPath)
    {
        return fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase);
    }
}

// Класс настроек
public class FileStorageOptions
{
    public string BasePath { get; set; } = "./uploads";
}