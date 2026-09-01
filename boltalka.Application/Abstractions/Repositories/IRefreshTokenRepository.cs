using boltalka.Application.Models.RefreshToken;

namespace boltalka.Application.Abstractions.Repositories;

public interface IRefreshTokenRepository: IRepository<RefreshToken>
{
    /// <summary>
    /// Получить старый токен.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Refresh токен.</returns>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
}