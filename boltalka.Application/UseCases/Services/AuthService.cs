using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Abstractions.Services;
using boltalka.Application.Models;
using boltalka.Application.Models.Auth;
using boltalka.Application.Models.RefreshToken;
using boltalka.Application.Models.User;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace boltalka.Application.UseCases.Services;

public class AuthService : IAuthService
{   
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<Result<AuthTokens>> RegisterAsync(Register register, CancellationToken cancellationToken)
    {
        if (await _userRepository.LoginExistsAsync(register.Login, cancellationToken))
            return Result<AuthTokens>.Failure("Логин уже занят.");
        
        var user = _mapper.Map<User>(register);
        
        await _userRepository.AddAsync(user, cancellationToken);
        
        return await GenerateTokensAsync(user, cancellationToken);
    }

    public async Task<Result<AuthTokens>> LoginAsync(Credentials credentials, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByLoginAsync(credentials.Login, cancellationToken);
        
        if (user is null || !BCrypt.Net.BCrypt.Verify(credentials.Password, user.PasswordHash))
            return Result<AuthTokens>.Failure("Неверный логин или пароль.");

        if (!user.IsActive)
            return Result<AuthTokens>.Failure("Аккаунт не активен.");

        return await GenerateTokensAsync(user, cancellationToken);
    }

    public async Task<Result<AuthTokens>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        if (storedToken is null || storedToken.ExpiresAt < DateTime.UtcNow)
            return Result<AuthTokens>.Failure("Недействительный или истекший refresh-токен.");

        await _refreshTokenRepository.DeleteAsync(storedToken, cancellationToken);

        var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
        
        if (user is null) return Result<AuthTokens>.Failure("Пользователь не найден.");

        return await GenerateTokensAsync(user, cancellationToken);
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {        
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        
        if (storedToken is not null)
            await _refreshTokenRepository.DeleteAsync(storedToken, cancellationToken);

        return Result.Success();
    }
    
    private async Task<Result<AuthTokens>> GenerateTokensAsync(User user, CancellationToken ct)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshTokenValue = GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(
                Convert.ToDouble(_configuration["Jwt:RefreshTokenLifetimeDays"] ?? "7")),
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        };
        
        await _refreshTokenRepository.AddAsync(refreshToken, ct);

        return Result<AuthTokens>.Success(new AuthTokens
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(
                _configuration["Jwt:AccessTokenLifetimeMinutes"] ?? "15"))
        });
    }

    private string GenerateAccessToken(User user)
    {
        var secretKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Login),
            new Claim("displayName", user.DisplayName)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(
                _configuration["Jwt:AccessTokenLifetimeMinutes"] ?? "15")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }
}