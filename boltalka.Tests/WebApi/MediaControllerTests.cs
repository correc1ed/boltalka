using System.Security.Claims;
using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models.Media;
using boltalka.Application.Models.User;
using boltalka.Tests.Infrastructure;
using boltalka.Contracts.Models.Media;
using boltalka.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Tests.WebApi;

public class MediaControllerTests : TestBase
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IUserRepository _userRepository;
    private readonly MediaController _controller;

    public MediaControllerTests()
    {
        var mediaService = ServiceProvider.GetRequiredService<IMediaService>();
        _mediaRepository = ServiceProvider.GetRequiredService<IMediaRepository>();
        _userRepository = ServiceProvider.GetRequiredService<IUserRepository>();
        var mapper = ServiceProvider.GetRequiredService<IMapper>();
        _controller = new MediaController(mediaService, mapper);
    }

    private void SetupAuthenticatedUser(Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "Test");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

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
        await _userRepository.AddAsync(user, CancellationToken.None);
        return user.Id;
    }

    private async Task<Media> CreateMediaAsync(Guid uploaderUserId, string fileName = "test.png", string contentType = "image/png")
    {
        var media = new Media
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = 1234,
            StoragePath = $"fake/path/{Guid.NewGuid()}",
            UploadedByUserId = uploaderUserId,
            CreatedAt = DateTime.UtcNow
        };
        await _mediaRepository.AddAsync(media, CancellationToken.None);
        return media;
    }

    private IFormFile CreateMockFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    // ==================== Upload ====================

    [Fact]
    public async Task Upload_ValidFile_ReturnsOkWithMedia()
    {
        // Arrange
        var userId = await CreateUserAsync("uploader");
        SetupAuthenticatedUser(userId);
        var file = CreateMockFile("test.png", "image/png", [ 1, 2, 3]);

        // Act
        var result = await _controller.UploadAsync(file, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var mediaContract = Assert.IsType<MediaContract>(okResult.Value);
        Assert.Equal("test.png", mediaContract.FileName);
        Assert.Equal("image/png", mediaContract.ContentType);
        Assert.Equal(3, mediaContract.SizeBytes);
    }

    [Fact]
    public async Task Upload_EmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("uploader");
        SetupAuthenticatedUser(userId);
        var file = CreateMockFile("empty.png", "image/png", []);

        // Act
        var result = await _controller.UploadAsync(file, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Upload_NullFile_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("uploader");
        SetupAuthenticatedUser(userId);

        // Act
        var result = await _controller.UploadAsync(null, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Upload_InvalidContentType_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("uploader");
        SetupAuthenticatedUser(userId);
        var file = CreateMockFile("bad.exe", "application/x-MsDownload", [1, 2, 3]);

        // Act
        var result = await _controller.UploadAsync(file, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Upload_ExceedsMaxSize_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("uploader");
        SetupAuthenticatedUser(userId);
        var bigData = new byte[51 * 1024 * 1024]; // 51 MB
        var file = CreateMockFile("big.png", "image/png", bigData);

        // Act
        var result = await _controller.UploadAsync(file, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== GetMediaInfo ====================

    [Fact]
    public async Task GetMediaInfo_ExistingMedia_ReturnsOkWithMedia()
    {
        // Arrange
        var userId = await CreateUserAsync("owner");
        var media = await CreateMediaAsync(userId);
        SetupAuthenticatedUser(userId); // не обязательно, метод не использует текущего пользователя

        // Act
        var result = await _controller.GetMediaInfoAsync(media.Id, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var mediaContract = Assert.IsType<MediaContract>(okResult.Value);
        Assert.Equal(media.Id, mediaContract.Id);
        Assert.Equal("test.png", mediaContract.FileName);
    }

    [Fact]
    public async Task GetMediaInfo_NonExistingMedia_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("user");
        SetupAuthenticatedUser(userId);

        // Act
        var result = await _controller.GetMediaInfoAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ==================== Download ====================

    [Fact]
    public async Task Download_ExistingMedia_ReturnsFileStreamResult()
    {
        // Arrange
        var userId = await CreateUserAsync("owner");
        var media = await CreateMediaAsync(userId);
        SetupAuthenticatedUser(userId);

        // Act
        var result = await _controller.DownloadAsync(media.Id, CancellationToken.None);

        // Assert
        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal(media.FileName, fileResult.FileDownloadName);
        Assert.Equal(media.ContentType, fileResult.ContentType);
    }

    [Fact]
    public async Task Download_NonExistingMedia_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("user");
        SetupAuthenticatedUser(userId);

        // Act
        var result = await _controller.DownloadAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== DeleteMedia ====================

    [Fact]
    public async Task DeleteMedia_Owner_ReturnsOk()
    {
        // Arrange
        var ownerId = await CreateUserAsync("owner");
        var media = await CreateMediaAsync(ownerId);
        SetupAuthenticatedUser(ownerId);

        // Act
        var result = await _controller.DeleteMediaAsync(media.Id, CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);
        var exists = await _mediaRepository.GetByIdAsync(media.Id, CancellationToken.None);
        Assert.Null(exists);
    }

    [Fact]
    public async Task DeleteMedia_NotOwner_ReturnsBadRequest()
    {
        // Arrange
        var ownerId = await CreateUserAsync("owner");
        var otherUserId = await CreateUserAsync("other");
        var media = await CreateMediaAsync(ownerId);
        SetupAuthenticatedUser(otherUserId);

        // Act
        var result = await _controller.DeleteMediaAsync(media.Id, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteMedia_NonExistingMedia_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("user");
        SetupAuthenticatedUser(userId);

        // Act
        var result = await _controller.DeleteMediaAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}