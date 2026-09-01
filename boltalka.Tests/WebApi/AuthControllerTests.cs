using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models.Auth;
using boltalka.Tests.Infrastructure;
using boltalka.Contracts.Models.Auth;
using boltalka.Infrastructure.Database;
using boltalka.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace boltalka.Tests.WebApi;

public class AuthControllerTests : TestBase
{
    private readonly IAuthService _authService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authService = ServiceProvider.GetRequiredService<IAuthService>();
        var mapper = ServiceProvider.GetRequiredService<IMapper>();
        _controller = new AuthController(_authService, mapper);
    }

    private void SetupAuthenticatedUser(Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ==================== Register ====================

    [Fact]
    public async Task Register_ValidData_ReturnsOkWithTokens()
    {
        // Arrange
        var registerContract = new RegisterContract
        {
            Login = "NewUser",
            Password = "StrongPass1!",
            DisplayName = "New User"
        };

        // Act
        var result = await _controller.RegisterAsync(registerContract, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var tokens = Assert.IsType<AuthTokensContract>(okResult.Value);
        Assert.False(string.IsNullOrEmpty(tokens.AccessToken));
        Assert.False(string.IsNullOrEmpty(tokens.RefreshToken));

        // Проверяем, что пользователь создан в БД
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Login == "NewUser");
        Assert.NotNull(user);
    }

    [Fact]
    public async Task Register_ExistingLogin_ReturnsBadRequest()
    {
        // Arrange
        await _authService.RegisterAsync(new Register
        {
            Login = "existing",
            Password = "Pass123!",
            DisplayName = "Existing"
        }, CancellationToken.None);

        var registerContract = new RegisterContract
        {
            Login = "existing",
            Password = "AnotherPass1!",
            DisplayName = "Another"
        };

        // Act
        var result = await _controller.RegisterAsync(registerContract, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("занят", badRequest.Value?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    // ==================== Login ====================

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithTokens()
    {
        // Arrange
        await _authService.RegisterAsync(new Register
        {
            Login = "LoginUser",
            Password = "Pass123!",
            DisplayName = "Login User"
        }, CancellationToken.None);

        var loginContract = new CredentialsContract
        {
            Login = "LoginUser",
            Password = "Pass123!"
        };

        // Act
        var result = await _controller.LoginAsync(loginContract, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var tokens = Assert.IsType<AuthTokensContract>(okResult.Value);
        Assert.False(string.IsNullOrEmpty(tokens.AccessToken));
        Assert.False(string.IsNullOrEmpty(tokens.RefreshToken));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsBadRequest()
    {
        // Arrange
        await _authService.RegisterAsync(new Register
        {
            Login = "LoginUser2",
            Password = "Pass123!",
            DisplayName = "Login User 2"
        }, CancellationToken.None);

        var loginContract = new CredentialsContract
        {
            Login = "LoginUser2",
            Password = "WrongPassword"
        };

        // Act
        var result = await _controller.LoginAsync(loginContract, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_NonExistingUser_ReturnsBadRequest()
    {
        // Arrange
        var loginContract = new CredentialsContract
        {
            Login = "NoUser",
            Password = "Whatever"
        };

        // Act
        var result = await _controller.LoginAsync(loginContract, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_InactiveUser_ReturnsBadRequest()
    {
        // Arrange
        var register = new Register
        {
            Login = "inactive",
            Password = "Pass123!",
            DisplayName = "Inactive"
        };
        await _authService.RegisterAsync(register, CancellationToken.None);
        // Деактивируем пользователя через сервис (или напрямую через репозиторий)
        using (var scope = ServiceProvider.CreateScope())
        {
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var user = await userRepo.GetByLoginAsync("inactive", CancellationToken.None);
            user!.IsActive = false;
            await userRepo.UpdateAsync(user, CancellationToken.None);
        }

        var loginContract = new CredentialsContract
        {
            Login = "inactive",
            Password = "Pass123!"
        };

        // Act
        var result = await _controller.LoginAsync(loginContract, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ==================== Refresh ====================

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewTokens()
    {
        // Arrange
        var register = new Register { Login = "RefreshUser", Password = "Pass123!", DisplayName = "Refresh" };
        var regResult = await _authService.RegisterAsync(register, CancellationToken.None);
        Assert.NotNull(regResult.Value);
        var oldRefresh = regResult.Value.RefreshToken;

        // Act
        var result = await _controller.RefreshAsync(oldRefresh, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var tokens = Assert.IsType<AuthTokensContract>(okResult.Value);
        Assert.NotEqual(oldRefresh, tokens.RefreshToken);
        Assert.False(string.IsNullOrEmpty(tokens.AccessToken));
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.RefreshAsync("invalid-token", CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ==================== Logout ====================

    [Fact]
    public async Task Logout_AuthenticatedUser_ReturnsOk()
    {
        // Arrange
        var register = new Register { Login = "LogoutUser", Password = "Pass123!", DisplayName = "Logout" };
        var regResult = await _authService.RegisterAsync(register, CancellationToken.None);
        var refreshToken = regResult.Value!.RefreshToken;
        var userId = GetUserIdFromToken(regResult.Value.AccessToken);
        SetupAuthenticatedUser(userId);

        // Act
        var result = await _controller.LogoutAsync(refreshToken, CancellationToken.None);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    private Guid GetUserIdFromToken(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);
        var sub = jwt.Subject;
        return Guid.Parse(sub);
    }
}