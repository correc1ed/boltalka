using System.Linq;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models.Auth;
using boltalka.Application.Models.RefreshToken;
using boltalka.Application.Models.User;
using boltalka.Application.Tests.Infrastructure;
using boltalka.Application.UseCases.Services;
using boltalka.Infrastructure.Database;
using boltalka.Infrastructure.Database.Entities;
using boltalka.Infrastructure.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Tests.Services;

public class AuthServiceTests : TestBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthServiceTests()
    {
        _authService = ServiceProvider.GetRequiredService<IAuthService>();
        _userRepository = ServiceProvider.GetRequiredService<IUserRepository>();
        _refreshTokenRepository = ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuthService, AuthService>();
    }

    // ==================== RegisterAsync ====================

    [Fact]
    public async Task Register_NewUser_ReturnsTokens()
    {
        var dto = new Register
        {
            Login = "newuser",
            Password = "StrongPass1!",
            DisplayName = "New User"
        };

        var result = await _authService.RegisterAsync(dto, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrEmpty(result.Value.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.Value.RefreshToken));

        // Проверяем, что пользователь создан в БД
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Login == "newuser");
        Assert.NotNull(user);
        Assert.True(BCrypt.Net.BCrypt.Verify("StrongPass1!", user.PasswordHash));
        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task Register_ExistingLogin_ReturnsFailure()
    {
        // Создаём пользователя напрямую
        await _userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Login = "existing",
            DisplayName = "Existing",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var dto = new Register { Login = "existing", Password = "AnyPass1!", DisplayName = "Dup" };
        var result = await _authService.RegisterAsync(dto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("занят", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== LoginAsync ====================

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        await _userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Login = "logintest",
            DisplayName = "Test",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Secret123"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var creds = new Credentials { Login = "logintest", Password = "Secret123" };
        var result = await _authService.LoginAsync(creds, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrEmpty(result.Value.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.Value.RefreshToken));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsFailure()
    {
        await _userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Login = "user",
            DisplayName = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Correct"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var creds = new Credentials { Login = "user", Password = "Wrong" };
        var result = await _authService.LoginAsync(creds, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Неверный логин или пароль", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_NonExistingUser_ReturnsFailure()
    {
        var creds = new Credentials { Login = "nobody", Password = "pass" };
        var result = await _authService.LoginAsync(creds, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Неверный логин или пароль", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_InactiveUser_ReturnsFailure()
    {
        await _userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Login = "inactive",
            DisplayName = "Inactive",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"),
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var creds = new Credentials { Login = "inactive", Password = "pass" };
        var result = await _authService.LoginAsync(creds, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("не активен", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== RefreshTokenAsync ====================

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsNewTokens()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = "refreshuser",
            DisplayName = "Refresh",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userRepository.AddAsync(user, CancellationToken.None);

        var oldRefresh = "valid-refresh-token";
        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = oldRefresh,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var result = await _authService.RefreshTokenAsync(oldRefresh, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(oldRefresh, result.Value.RefreshToken); // новый refresh-токен
        Assert.False(string.IsNullOrEmpty(result.Value.AccessToken));

        // Старый токен должен быть удалён
        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var oldTokenExists = await db.RefreshTokens.AnyAsync(rt => rt.Token == oldRefresh);
        Assert.False(oldTokenExists);
    }

    [Fact]
    public async Task RefreshToken_ExpiredToken_ReturnsFailure()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = "expireduser",
            DisplayName = "Expired",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userRepository.AddAsync(user, CancellationToken.None);

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "expired-token",
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // истёк
            CreatedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var result = await _authService.RefreshTokenAsync("expired-token", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Недействительный или истекший", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RefreshToken_NonExistingToken_ReturnsFailure()
    {
        var result = await _authService.RefreshTokenAsync("nonexistent", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Недействительный или истекший", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== LogoutAsync ====================

    [Fact]
    public async Task Logout_ValidToken_RemovesToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = "logoutuser",
            DisplayName = "Logout",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await _userRepository.AddAsync(user, CancellationToken.None);

        var token = "logout-token";
        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var result = await _authService.LogoutAsync(token, CancellationToken.None);

        Assert.True(result.IsSuccess);

        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var exists = await db.RefreshTokens.AnyAsync(rt => rt.Token == token);
        Assert.False(exists);
    }

    [Fact]
    public async Task Logout_NonExistingToken_ReturnsSuccess()
    {
        // Выход с несуществующим токеном не должен быть ошибкой
        var result = await _authService.LogoutAsync("fake-token", CancellationToken.None);
        Assert.True(result.IsSuccess);
    }
}