using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models.Media;
using boltalka.Application.Models.User;
using boltalka.Application.Tests.Infrastructure;
using boltalka.Application.UseCases.Services;
using boltalka.Infrastructure.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Tests.Services;

public class UserServiceTests : TestBase
{
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepo;
    private readonly IMediaRepository _mediaRepository;

    public UserServiceTests()
    {
        _userService = ServiceProvider.GetRequiredService<IUserService>();
        _userRepo = ServiceProvider.GetRequiredService<IUserRepository>();
        _mediaRepository = ServiceProvider.GetRequiredService<IMediaRepository>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IUserService, UserService>();
    }

    // ==================== GetProfileAsync ====================

    [Fact]
    public async Task GetProfile_ExistingUser_ReturnsProfile()
    {
        var userId = await CreateUserReturnId("login1", "Alice", ct: CancellationToken.None);
        var result = await _userService.GetProfileAsync(userId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var profile = result.Value;
        Assert.NotNull(profile);
        Assert.Equal("login1", profile.Login);
        Assert.Equal("Alice", profile.DisplayName);
    }

    [Fact]
    public async Task GetProfile_NonExistingUser_ReturnsFailure()
    {
        var result = await _userService.GetProfileAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    // ==================== GetUserByIdAsync ====================

    [Fact]
    public async Task GetUserById_ExistingUser_ReturnsProfile()
    {
        var userId = await CreateUserReturnId("login2", "Bob", ct: CancellationToken.None);
        var result = await _userService.GetUserByIdAsync(userId, CancellationToken.None);
        Assert.True(result.IsSuccess);
        var user = result.Value;
        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
    }

    [Fact]
    public async Task GetUserById_NonExistingUser_ReturnsFailure()
    {
        var result = await _userService.GetUserByIdAsync(Guid.NewGuid(), CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    // ==================== UpdateDisplayNameAsync ====================

    [Fact]
    public async Task UpdateDisplayName_ValidName_UpdatesSuccessfully()
    {
        var userId = await CreateUserReturnId("login3", "OldName", ct: CancellationToken.None);
        var result = await _userService.UpdateDisplayNameAsync(userId, "NewName", CancellationToken.None);
        Assert.True(result.IsSuccess);
        var updatedProfile = result.Value;
        Assert.NotNull(updatedProfile);
        Assert.Equal("NewName", updatedProfile.DisplayName);

        var profileResult = await _userService.GetProfileAsync(userId, CancellationToken.None);
        Assert.True(profileResult.IsSuccess);
        var profile = profileResult.Value;
        Assert.NotNull(profile);
        Assert.Equal("NewName", profile.DisplayName);
    }

    [Fact]
    public async Task UpdateDisplayName_EmptyName_ReturnsFailure()
    {
        var userId = await CreateUserReturnId("login4", "SomeName", ct: CancellationToken.None);
        var result = await _userService.UpdateDisplayNameAsync(userId, "", CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateDisplayName_NonExistingUser_ReturnsFailure()
    {
        var result = await _userService.UpdateDisplayNameAsync(Guid.NewGuid(), "Name", CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    // ==================== SetAvatarAsync ====================

    [Fact]
    public async Task SetAvatar_ValidImage_UpdatesAvatar()
    {
        var userId = await CreateUserReturnId("login5", "AvatarUser", ct: CancellationToken.None);
        var media = await CreateMediaAsync("image/png", ct: CancellationToken.None);

        var result = await _userService.SetAvatarAsync(userId, media.Id, CancellationToken.None);
        
        Assert.True(result.IsSuccess);
        var profile = result.Value;
        Assert.NotNull(profile);
        Assert.NotNull(profile.AvatarUrl);
        Assert.Equal(media.StoragePath, profile.AvatarUrl);
    }

    [Fact]
    public async Task SetAvatar_NonImageType_ReturnsFailure()
    {
        var userId = await CreateUserReturnId("login6", "BadAvatar", ct: CancellationToken.None);
        var media = await CreateMediaAsync("application/pdf", ct: CancellationToken.None);
        var result = await _userService.SetAvatarAsync(userId, media.Id, CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task SetAvatar_NonExistingMedia_ReturnsFailure()
    {
        var userId = await CreateUserReturnId("login7", "NoMedia", ct: CancellationToken.None);
        var result = await _userService.SetAvatarAsync(userId, Guid.NewGuid(), CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task SetAvatar_NonExistingUser_ReturnsFailure()
    {
        var media = await CreateMediaAsync("image/jpeg", ct: CancellationToken.None);
        var result = await _userService.SetAvatarAsync(Guid.NewGuid(), media.Id, CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    // ==================== SearchUsersAsync ====================

    [Fact]
    public async Task SearchUsers_MatchingQuery_ReturnsUsers()
    {
        await CreateUserReturnId("alice", "Alice", ct: CancellationToken.None);
        await CreateUserReturnId("bob", "Bob", ct: CancellationToken.None);
        await CreateUserReturnId("alicia", "Alicia", ct: CancellationToken.None);

        var result = await _userService.SearchUsersAsync("ali", 0, 10, CancellationToken.None);
        Assert.True(result.IsSuccess);
        var users = result.Value;
        Assert.NotNull(users);
        var usersList = users.ToList(); // материализуем
        Assert.Equal(2, usersList.Count);
        Assert.Contains(usersList, u => u.Login == "alice");
        Assert.Contains(usersList, u => u.Login == "alicia");
    }

    [Fact]
    public async Task SearchUsers_NoMatch_ReturnsEmptyList()
    {
        await CreateUserReturnId("john", "John", ct: CancellationToken.None);
        var result = await _userService.SearchUsersAsync("xyz", 0, 10, CancellationToken.None);
        Assert.True(result.IsSuccess);
        var users = result.Value;
        Assert.NotNull(users);
        var usersList = users.ToList();
        Assert.Empty(usersList);
    }

    [Fact]
    public async Task SearchUsers_EmptyQuery_ReturnsFailure()
    {
        var result = await _userService.SearchUsersAsync("", 0, 10, CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task SearchUsers_Pagination_ReturnsCorrectPage()
    {
        for (int i = 1; i <= 20; i++)
            await CreateUserReturnId($"user{i}", $"User {i}", ct: CancellationToken.None);

        var page1 = await _userService.SearchUsersAsync("user", 0, 5, CancellationToken.None);
        var page2 = await _userService.SearchUsersAsync("user", 5, 5, CancellationToken.None);

        Assert.True(page1.IsSuccess);
        Assert.True(page2.IsSuccess);
        var users1 = page1.Value;
        var users2 = page2.Value;
        Assert.NotNull(users1);
        Assert.NotNull(users2);

        var list1 = users1.ToList();
        var list2 = users2.ToList();

        Assert.Equal(5, list1.Count);
        Assert.Equal(5, list2.Count);

        var ids1 = list1.Select(u => u.Id).ToHashSet();
        var ids2 = list2.Select(u => u.Id).ToHashSet();
        Assert.False(ids1.Overlaps(ids2));
    }

    // ==================== Deactivate / Activate ====================

    [Fact]
    public async Task DeactivateUser_ActiveUser_Deactivates()
    {
        var userId = await CreateUserReturnId("active1", "Active", ct: CancellationToken.None);
        var result = await _userService.DeactivateUserAsync(userId, CancellationToken.None);
        Assert.True(result.IsSuccess);

        var profile = await _userService.GetProfileAsync(userId, CancellationToken.None);
        Assert.True(profile.IsSuccess);
        var userProfile = profile.Value;
        Assert.NotNull(userProfile);
        Assert.False(userProfile.IsActive);
    }

    [Fact]
    public async Task DeactivateUser_AlreadyInactive_ReturnsFailure()
    {
        var userId = await CreateUserReturnId("inactive1", "Inactive", isActive: false, ct: CancellationToken.None);
        var result = await _userService.DeactivateUserAsync(userId, CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ActivateUser_InactiveUser_Activates()
    {
        var userId = await CreateUserReturnId("inactive2", "Inactive2", isActive: false, ct: CancellationToken.None);
        var result = await _userService.ActivateUserAsync(userId, CancellationToken.None);
        Assert.True(result.IsSuccess);

        var profile = await _userService.GetProfileAsync(userId, CancellationToken.None);
        Assert.True(profile.IsSuccess);
        var userProfile = profile.Value;
        Assert.NotNull(userProfile);
        Assert.True(userProfile.IsActive);
    }

    [Fact]
    public async Task ActivateUser_AlreadyActive_ReturnsFailure()
    {
        var userId = await CreateUserReturnId("active2", "Active2", ct: CancellationToken.None);
        var result = await _userService.ActivateUserAsync(userId, CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    // ==================== Вспомогательные методы ====================

    private async Task<Guid> CreateUserReturnId(string login, string displayName, bool isActive = true, CancellationToken ct = default)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = login,
            DisplayName = displayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        await _userRepo.AddAsync(user, ct);
        return user.Id;
    }

    private async Task<Media> CreateMediaAsync(string contentType, CancellationToken ct = default)
    {
        var media = new Media
        {
            Id = Guid.NewGuid(),
            FileName = "test.png",
            ContentType = contentType,
            SizeBytes = 12345,
            StoragePath = $"fake/path/{Guid.NewGuid()}",
            UploadedByUserId = null,
            CreatedAt = DateTime.UtcNow
        };
        await _mediaRepository.AddAsync(media, ct);
        return media;
    }
}