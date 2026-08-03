using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Abstractions.Storage;
using boltalka.Application.Models;
using boltalka.Application.Models.Media;
using Microsoft.Extensions.Logging;

namespace boltalka.Application.UseCases.Services;

public class MediaService : IMediaService
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IMapper _mapper;
    private readonly ILogger<MediaService> _logger;
    
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "application/pdf", "text/plain",
        "application/zip", "application/x-rar-compressed"
    };
    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 МБ
    
    public MediaService(
        IMediaRepository mediaRepository,
        IFileStorage fileStorage,
        IMapper mapper,
        ILogger<MediaService> logger)
    {
        _mediaRepository = mediaRepository;
        _fileStorage = fileStorage;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<Media>> UploadAsync(Guid uploaderUserId, Stream content, string fileName, string contentType,
        CancellationToken cancellationToken)
    {
        if (content == null || content.Length == 0)
            return Result<Media>.Failure("Файл пуст.");

        if (content.Length > MaxFileSizeBytes)
            return Result<Media>.Failure($"Размер файла превышает {MaxFileSizeBytes / (1024 * 1024)} МБ.");

        if (!AllowedContentTypes.Contains(contentType))
            return Result<Media>.Failure("Недопустимый тип файла.");

        string storagePath;
        try
        {
            storagePath = await _fileStorage.SaveAsync(fileName, content, contentType, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка сохранения файла {FileName}", fileName);
            return Result<Media>.Failure("Не удалось сохранить файл.");
        }

        var media = new Media
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = content.Length,
            StoragePath = storagePath,
            UploadedByUserId = uploaderUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _mediaRepository.AddAsync(media, cancellationToken);

        var result = _mapper.Map<Media>(media);
        return Result<Media>.Success(result);
    }

    public async Task<Result<Media>> GetMediaAsync(Guid mediaId, CancellationToken cancellationToken)
    {
        var media = await _mediaRepository.GetByIdAsync(mediaId, cancellationToken);
        
        if (media is null)
            return Result<Media>.Failure("Файл не найден.");

        return Result<Media>.Success(_mapper.Map<Media>(media));
    }

    public async Task<Result<Stream>> GetFileStreamAsync(Guid mediaId, Guid requestUserId, CancellationToken cancellationToken)
    {
        var media = await _mediaRepository.GetByIdAsync(mediaId, cancellationToken);
        if (media is null)
            return Result<Stream>.Failure("Файл не найден.");

        try
        {
            var stream = await _fileStorage.OpenReadAsync(media.StoragePath, cancellationToken);
            return Result<Stream>.Success(stream);
        }
        catch (FileNotFoundException)
        {
            _logger.LogWarning("Файл {StoragePath} отсутствует в хранилище", media.StoragePath);
            return Result<Stream>.Failure("Файл не найден в хранилище.");
        }
    }

    public async Task<Result> DeleteMediaAsync(Guid mediaId, Guid userId, CancellationToken cancellationToken)
    {
        var media = await _mediaRepository.GetByIdAsync(mediaId, cancellationToken);
        if (media is null)
            return Result.Failure("Файл не найден.");

        if (media.UploadedByUserId != userId)
            return Result.Failure("Недостаточно прав для удаления файла.");

        await _fileStorage.DeleteAsync(media.StoragePath, cancellationToken);

        await _mediaRepository.DeleteAsync(media, cancellationToken);

        return Result.Success();
    }
}