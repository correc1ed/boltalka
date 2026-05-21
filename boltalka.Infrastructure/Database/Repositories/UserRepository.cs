using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using boltalka.Application.Models.User;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly IMapper _mapper;

    public UserRepository(ServiceDbContext dbContext, IMapper mapper) : base(dbContext)
    {
        _mapper = mapper;
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        var resultUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Login == login);
        
        return _mapper.Map<User>(resultUser);
    }

    public async Task<bool> LoginExistsAsync(string login)
    {
        return await _dbContext.Users.AnyAsync(u => u.Login == login);
    }

    public async Task<IEnumerable<User>> GetUsersAsync(int skip, int take)
    {
        var resultUsers = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<User>>(resultUsers);
    }
}