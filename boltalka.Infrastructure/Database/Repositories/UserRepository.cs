using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Models.User;
using boltalka.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Repositories;

public class UserRepository : BaseRepository<UserEntity, User>, IUserRepository
{
    public UserRepository(ServiceDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
    {
    }

    public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken)
    {
        var resultUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Login == login, cancellationToken);
        
        return _mapper.Map<User>(resultUser);
    }

    public async Task<User?> GetFullInfoByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(user => user.Avatar)
            .Include(user => user.ChatMembers)
                .ThenInclude(chatMembers => chatMembers.Chat)
            .Include(user => user.Messages)
            .Include(user => user.Calls)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken); // тут снова не находит ни AvatarMediaId ни Avatar.
        
        return user is null ? null : _mapper.Map<User>(user);
    }

    public async Task<bool> LoginExistsAsync(string login, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.AnyAsync(u => u.Login == login, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetUsersAsync(int skip, int take, CancellationToken cancellationToken)
    {
        var resultUsers = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        
        return _mapper.Map<IEnumerable<User>>(resultUsers);
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string query, int skip, int take, CancellationToken cancellationToken = default)
    {
        var lowerQuery = query.ToLowerInvariant();
        
        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Login.ToLower().Contains(lowerQuery) || u.DisplayName.ToLower().Contains(lowerQuery))
            .OrderBy(u => u.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        
        return _mapper.Map<IEnumerable<User>>(users);
    }
}