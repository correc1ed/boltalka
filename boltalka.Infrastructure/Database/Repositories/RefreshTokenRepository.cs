using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Models.RefreshToken;
using boltalka.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshTokenEntity, RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ServiceDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
        
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken сancellationToken)
    {
        var refreshToken = await _dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token, сancellationToken);
        
        return _mapper.Map<RefreshToken>(refreshToken);
    }
}