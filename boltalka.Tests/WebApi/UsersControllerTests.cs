using System.Security.Claims;
using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models.Media;
using boltalka.Application.Models.User;
using boltalka.Tests.Infrastructure;
using boltalka.Contracts.Models.User;
using boltalka.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Tests.WebApi;

public class UsersControllerTests : TestBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        var userService = ServiceProvider.GetRequiredService<IUserService>();
        _userRepository = ServiceProvider.GetRequiredService<IUserRepository>();
        _mediaRepository = ServiceProvider.GetRequiredService<IMediaRepository>();
        var mapper = ServiceProvider.GetRequiredService<IMapper>();
        _controller = new UsersController(userService, mapper);
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

    private async Task<Guid> CreateUserAsync(string login, string? displayName = null, bool isActive = true)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = login,
            DisplayName = displayName ?? login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        await _userRepository.AddAsync(user, CancellationToken.None);
        return user.Id;
    }

    private async Task<Media> CreateMediaAsync(string contentType = "image/png", Guid? uploaderId = null)
    {
        var media = new Media
        {
            Id = Guid.NewGuid(),
            FileName = "avatar.png",
            ContentType = contentType,
            SizeBytes = 1234,
            StoragePath = $"fake/path/{Guid.NewGuid()}",
            UploadedByUserId = uploaderId,
            CreatedAt = DateTime.UtcNow
        };
        await _mediaRepository.AddAsync(media, CancellationToken.None);
        return media;
    }

    // ==================== GetMyProfile ====================

    [Fact]
    public async Task GetMyProfile_AuthenticatedUser_ReturnsOkWithProfile()
    {
        // Arrange
        var userId = await CreateUserAsync("user1", "Alice");
        SetupAuthenticatedUser(userId);

        // Act
        var result = await _controller.GetMyProfileAsync(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var profile = Assert.IsType<UserProfileContract>(okResult.Value);
        Assert.Equal(userId, profile.Id);
        Assert.Equal("user1", profile.Login);
        Assert.Equal("Alice", profile.DisplayName);
    }

    [Fact]
    public async Task GetMyProfile_UserNotFound_ReturnsBadRequest()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        SetupAuthenticatedUser(nonExistentUserId);

        // Act
        var result = await _controller.GetMyProfileAsync(CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ==================== GetUserById ====================

    [Fact]
    public async Task GetUserById_ExistingUser_ReturnsOkWithProfile()
    {
        // Arrange
        var userId = await CreateUserAsync("user2", "Bob");
        SetupAuthenticatedUser(Guid.NewGuid()); // не важно, метод не использует текущего пользователя

        // Act
        var result = await _controller.GetUserByIdAsync(userId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var profile = Assert.IsType<UserProfileContract>(okResult.Value);
        Assert.Equal(userId, profile.Id);
        Assert.Equal("Bob", profile.DisplayName);
    }

    [Fact]
    public async Task GetUserById_NonExistingUser_ReturnsBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser(Guid.NewGuid());

        // Act
        var result = await _controller.GetUserByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ==================== UpdateDisplayName ====================

    [Fact]
    public async Task UpdateDisplayName_ValidName_ReturnsOkWithUpdated()
    {
        // Arrange
        var userId = await CreateUserAsync("user3", "OldName");
        SetupAuthenticatedUser(userId);
        var newName = "NewName";

        // Act
        var result = await _controller.UpdateDisplayNameAsync(newName, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var profile = Assert.IsType<UserProfileContract>(okResult.Value);
        Assert.Equal(newName, profile.DisplayName);

        var dbUser = await _userRepository.GetByIdAsync(userId, CancellationToken.None);
        Assert.Equal(newName, dbUser!.DisplayName);
    }

    [Fact]
    public async Task UpdateDisplayName_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("user4", "SomeName");
        SetupAuthenticatedUser(userId);

        // Act
        var result = await _controller.UpdateDisplayNameAsync("", CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateDisplayName_NonExistingUser_ReturnsBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser(Guid.NewGuid());

        // Act
        var result = await _controller.UpdateDisplayNameAsync("NewName", CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ==================== SetAvatar ====================

    [Fact]
    public async Task SetAvatar_ValidImage_ReturnsOkWithUpdated()
    {
        // Arrange
        var userId = await CreateUserAsync("user5", "AvatarUser");
        var media = await CreateMediaAsync("image/png", userId);
        SetupAuthenticatedUser(userId);

        // Act
        var result = await _controller.SetAvatarAsync(media.Id, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var profile = Assert.IsType<UserProfileContract>(okResult.Value);
        Assert.Equal(media.StoragePath, profile.AvatarUrl);

        var dbUser = await _userRepository.GetByIdAsync(userId, CancellationToken.None);
        Assert.Equal(media.Id, dbUser!.AvatarMediaId);
    }

    [Fact]
    public async Task SetAvatar_NonImage_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("user6", "BadAvatar");
        var media = await CreateMediaAsync("application/pdf", userId);
        SetupAuthenticatedUser(userId);

        // Act
        var result = await _controller.SetAvatarAsync(media.Id, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task SetAvatar_NonExistingMedia_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("user7");
        SetupAuthenticatedUser(userId);

        // Act
        var result = await _controller.SetAvatarAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task SetAvatar_NonExistingUser_ReturnsBadRequest()
    {
        // Arrange
        var media = await CreateMediaAsync();
        SetupAuthenticatedUser(Guid.NewGuid());

        // Act
        var result = await _controller.SetAvatarAsync(media.Id, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ==================== SearchUsers ====================

    [Fact]
    public async Task SearchUsers_ValidQuery_ReturnsOkWithList()
    {
        // Arrange
        await CreateUserAsync("alice", "Alice");
        await CreateUserAsync("bob", "Bob");
        await CreateUserAsync("alex", "Alex");

        // Act
        var result = await _controller.SearchUsersAsync("al", 0, 10, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<UserProfileContract>>(okResult.Value);
        Assert.Equal(2, users.Count()); // alice, alex
    }

    [Fact]
    public async Task SearchUsers_NoResults_ReturnsOkEmpty()
    {
        // Arrange
        await CreateUserAsync("john", "John");

        // Act
        var result = await _controller.SearchUsersAsync("xyz", 0, 10, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<UserProfileContract>>(okResult.Value);
        Assert.Empty(users);
    }

    [Fact]
    public async Task SearchUsers_EmptyQuery_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SearchUsersAsync("", 0, 10, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ==================== DeactivateUser ====================

    [Fact]
    public async Task DeactivateUser_ActiveUser_ReturnsOk()
    {
        // Arrange
        var userId = await CreateUserAsync("active", "Active");
        SetupAuthenticatedUser(Guid.NewGuid()); // не важно, метод не использует текущего пользователя

        // Act
        var result = await _controller.DeactivateUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);

        var dbUser = await _userRepository.GetByIdAsync(userId, CancellationToken.None);
        Assert.False(dbUser!.IsActive);
    }

    [Fact]
    public async Task DeactivateUser_AlreadyInactive_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("inactive", "Inactive", isActive: false);
        SetupAuthenticatedUser(Guid.NewGuid());

        // Act
        var result = await _controller.DeactivateUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeactivateUser_NonExistingUser_ReturnsBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser(Guid.NewGuid());

        // Act
        var result = await _controller.DeactivateUserAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==================== ActivateUser ====================

    [Fact]
    public async Task ActivateUser_InactiveUser_ReturnsOk()
    {
        // Arrange
        var userId = await CreateUserAsync("inactive2", "Inactive2", isActive: false);
        SetupAuthenticatedUser(Guid.NewGuid());

        // Act
        var result = await _controller.ActivateUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);

        var dbUser = await _userRepository.GetByIdAsync(userId, CancellationToken.None);
        Assert.True(dbUser!.IsActive);
    }

    [Fact]
    public async Task ActivateUser_AlreadyActive_ReturnsBadRequest()
    {
        // Arrange
        var userId = await CreateUserAsync("active2", "Active2");
        SetupAuthenticatedUser(Guid.NewGuid());

        // Act
        var result = await _controller.ActivateUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ActivateUser_NonExistingUser_ReturnsBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser(Guid.NewGuid());

        // Act
        var result = await _controller.ActivateUserAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}