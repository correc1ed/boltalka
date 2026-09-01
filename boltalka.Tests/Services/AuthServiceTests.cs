using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models.Auth;
using boltalka.Application.Models.RefreshToken;
using boltalka.Application.Models.User;
using boltalka.Tests.Infrastructure;
using boltalka.Application.UseCases.Services;
using boltalka.Infrastructure.Database;
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
            Login = "new_user",
            Password = "StrongPass1!",
            DisplayName = "New User"
        };

        var result = await _authService.RegisterAsync(dto, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var tokens = result.Value;
        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrEmpty(tokens.AccessToken));
        Assert.False(string.IsNullOrEmpty(tokens.RefreshToken));

        using var scope = ServiceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Login == "new_user");
        Assert.NotNull(user);
        Assert.True(BCrypt.Net.BCrypt.Verify("StrongPass1!", user.PasswordHash));
        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task Register_ExistingLogin_ReturnsFailure()
    {
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
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("занят", error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== LoginAsync ====================

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        await _userRepository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Login = "login_test",
            DisplayName = "Test",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Secret123"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var credentials = new Credentials { Login = "login_test", Password = "Secret123" };
        var result = await _authService.LoginAsync(credentials, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var tokens = result.Value;
        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrEmpty(tokens.AccessToken));
        Assert.False(string.IsNullOrEmpty(tokens.RefreshToken));
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

        var credentials = new Credentials { Login = "user", Password = "Wrong" };
        var result = await _authService.LoginAsync(credentials, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("Неверный логин или пароль", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_NonExistingUser_ReturnsFailure()
    {
        var credentials = new Credentials { Login = "nobody", Password = "pass" };
        var result = await _authService.LoginAsync(credentials, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("Неверный логин или пароль", error, StringComparison.OrdinalIgnoreCase);
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

        var credentials = new Credentials { Login = "inactive", Password = "pass" };
        var result = await _authService.LoginAsync(credentials, CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("не активен", error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== RefreshTokenAsync ====================

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsNewTokens()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = "refresh_user",
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
        var tokens = result.Value;
        Assert.NotNull(tokens);
        Assert.NotEqual(oldRefresh, tokens.RefreshToken);
        Assert.False(string.IsNullOrEmpty(tokens.AccessToken));

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
            Login = "expired_user",
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
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var result = await _authService.RefreshTokenAsync("expired-token", CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("Недействительный или истекший", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RefreshToken_NonExistingToken_ReturnsFailure()
    {
        var result = await _authService.RefreshTokenAsync("nonexistent", CancellationToken.None);

        Assert.False(result.IsSuccess);
        var error = result.Error;
        Assert.NotNull(error);
        Assert.Contains("Недействительный или истекший", error, StringComparison.OrdinalIgnoreCase);
    }

    // ==================== LogoutAsync ====================

    [Fact]
    public async Task Logout_ValidToken_RemovesToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = "logout_user",
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
        var result = await _authService.LogoutAsync("fake-token", CancellationToken.None);
        Assert.True(result.IsSuccess);
    }
}