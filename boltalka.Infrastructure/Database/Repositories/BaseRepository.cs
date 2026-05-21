using boltalka.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Repositories;

public class BaseRepository<T> : IRepository<T> where T : class
{    
    protected readonly ServiceDbContext _dbContext;

    protected BaseRepository(ServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbContext.Set<T>().AsNoTracking().ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbContext.Set<T>().AddAsync(entity);
    }

    public virtual void Update(T entity)
    {
        _dbContext.Set<T>().Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
    }
}