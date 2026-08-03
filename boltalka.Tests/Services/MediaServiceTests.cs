using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models.Media;
using boltalka.Application.Models.User;
using boltalka.Application.Tests.Infrastructure;
using boltalka.Application.UseCases.Services;
using boltalka.Infrastructure.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Tests.Services;

public class MediaServiceTests : TestBase
{
    private readonly IMediaService _mediaService;
    private readonly IMediaRepository _mediaRepo;
    private readonly IUserRepository _userRepo;

    public MediaServiceTests()
    {
        _mediaService = ServiceProvider.GetRequiredService<IMediaService>();
        _mediaRepo = ServiceProvider.GetRequiredService<IMediaRepository>();
        _userRepo = ServiceProvider.GetRequiredService<IUserRepository>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMediaService, MediaService>();
    }

    // ==================== UploadAsync ====================

    [Fact]
    public async Task Upload_ValidFile_ReturnsMediaDto()
    {
        // Arrange
        var userId = await CreateUserAsync("uploader");
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        // Act
        var result = await _mediaService.UploadAsync(userId, stream, "test.png", "image/png", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test.png", result.Value.FileName);
        Assert.Equal("image/png", result.Value.ContentType);
        Assert.Equal(5, result.Value.SizeBytes);
        Assert.NotNull(result.Value.StoragePath);
    }

    [Fact]
    public async Task Upload_EmptyStream_ReturnsFailure()
    {
        var userId = await CreateUserAsync("uploader");
        using var stream = new MemoryStream(Array.Empty<byte>());

        var result = await _mediaService.UploadAsync(userId, stream, "empty.png", "image/png", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("пуст", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Upload_InvalidContentType_ReturnsFailure()
    {
        var userId = await CreateUserAsync("uploader");
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        var result = await _mediaService.UploadAsync(userId, stream, "bad.exe", "application/x-msdownload", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Недопустимый тип", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Upload_ExceedsMaxSize_ReturnsFailure()
    {
        var userId = await CreateUserAsync("uploader");
        var bigData = new byte[51 * 1024 * 1024];
        using var stream = new MemoryStream(bigData);

        var result = await _mediaService.UploadAsync(userId, stream, "big.png", "image/png", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("размер", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== GetMediaAsync ====================

    [Fact]
    public async Task GetMedia_ExistingMedia_ReturnsMediaDto()
    {
        var media = await CreateMediaAsync("file.pdf", "application/pdf");

        var result = await _mediaService.GetMediaAsync(media.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(media.Id, result.Value.Id);
        Assert.Equal("file.pdf", result.Value.FileName);
    }

    [Fact]
    public async Task GetMedia_NonExistingMedia_ReturnsFailure()
    {
        var result = await _mediaService.GetMediaAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Contains("не найден", result.Error);
    }

    // ==================== GetFileStreamAsync ====================

    [Fact]
    public async Task GetFileStream_ExistingMedia_ReturnsStream()
    {
        var media = await CreateMediaAsync("test.png", "image/png");

        var result = await _mediaService.GetFileStreamAsync(media.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        result.Value.Dispose();
    }

    [Fact]
    public async Task GetFileStream_NonExistingMedia_ReturnsFailure()
    {
        var result = await _mediaService.GetFileStreamAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    // ==================== DeleteMediaAsync ====================

    [Fact]
    public async Task DeleteMedia_Owner_RemovesMedia()
    {
        var ownerId = await CreateUserAsync("owner");
        var media = await CreateMediaAsync("file.txt", "text/plain", ownerId);

        var result = await _mediaService.DeleteMediaAsync(media.Id, ownerId, CancellationToken.None);
        Assert.True(result.IsSuccess);

        var getResult = await _mediaService.GetMediaAsync(media.Id, CancellationToken.None);
        Assert.False(getResult.IsSuccess);
    }

    [Fact]
    public async Task DeleteMedia_NotOwner_ReturnsFailure()
    {
        var ownerId = await CreateUserAsync("owner");
        var otherUserId = await CreateUserAsync("other");
        var media = await CreateMediaAsync("file.txt", "text/plain", ownerId);

        var result = await _mediaService.DeleteMediaAsync(media.Id, otherUserId, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Contains("прав", result.Error);
    }

    [Fact]
    public async Task DeleteMedia_NonExistingMedia_ReturnsFailure()
    {
        var result = await _mediaService.DeleteMediaAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    // ==================== Вспомогательные методы ====================

    private async Task<Guid> CreateUserAsync(string login)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = login,
            DisplayName = login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userRepo.AddAsync(user, cancellationToken: CancellationToken.None);
        return user.Id;
    }

    private async Task<Media> CreateMediaAsync(string fileName, string contentType, Guid? uploadedByUserId = null)
    {
        var media = new Media
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = 1234,
            StoragePath = $"fake/path/{Guid.NewGuid()}",
            UploadedByUserId = uploadedByUserId,
            CreatedAt = DateTime.UtcNow
        };
        await _mediaRepo.AddAsync(media, cancellationToken: CancellationToken.None);
        return media;
    }
}