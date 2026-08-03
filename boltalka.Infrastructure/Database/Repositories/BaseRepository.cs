using AutoMapper;
using boltalka.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database.Repositories;

public abstract class BaseRepository<TEntity, TModel> : IRepository<TModel>
    where TEntity : class
    where TModel : class
{
    protected readonly ServiceDbContext _dbContext;
    protected readonly IMapper _mapper;

    protected BaseRepository(ServiceDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    protected virtual IQueryable<TEntity> GetQueryable()
    {
        return _dbContext.Set<TEntity>().AsNoTracking();
    }

    public virtual async Task<TModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
        
        return entity is null ? null : _mapper.Map<TModel>(entity);
    }

    public virtual async Task<IEnumerable<TModel>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entities = await GetQueryable().ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<TModel>>(entities);
    }

    public virtual async Task AddAsync(TModel model, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<TEntity>(model);
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
       //_mapper.Map(entity, model);
    }

    public virtual async Task UpdateAsync(TModel model, CancellationToken cancellationToken)
    {
        var id = (Guid)typeof(TModel).GetProperty("Id")!.GetValue(model)!;
        var trackedEntity = await GetTrackedByIdAsync(id, cancellationToken);
        if (trackedEntity is null)
            throw new InvalidOperationException($"Сущность с Id={id} не найдена.");

        CopyScalarProperties(model, trackedEntity);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(TModel model, CancellationToken cancellationToken)
    {
        var id = (Guid)typeof(TModel).GetProperty("Id")!.GetValue(model)!;
        var trackedEntity = await GetTrackedByIdAsync(id, cancellationToken);
        if (trackedEntity != null)
        {
            _dbContext.Set<TEntity>().Remove(trackedEntity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
    
    private async Task<TEntity?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Set<TEntity>()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, ct);
    }
    
    private static readonly HashSet<Type> CopyableTypes = new()
    {
        typeof(string),
        typeof(DateTime), typeof(DateTime?),
        typeof(Guid), typeof(Guid?),
        typeof(int), typeof(int?),
        typeof(long), typeof(long?),
        typeof(bool), typeof(bool?),
        typeof(decimal), typeof(decimal?),
        typeof(double), typeof(double?),
        typeof(byte[]),
    };
    
    private void CopyScalarProperties(TModel source, TEntity destination)
    {
        var entityType = typeof(TEntity);
        var modelType = typeof(TModel);

        foreach (var prop in modelType.GetProperties())
        {
            if (prop.Name == "Id") continue;

            var entityProp = entityType.GetProperty(prop.Name);
            if (entityProp == null) continue;

            if (!IsCopyableType(entityProp.PropertyType))
                continue;

            var value = prop.GetValue(source);
            entityProp.SetValue(destination, value);
        }
    }

    private bool IsCopyableType(Type type)
    {
        if (CopyableTypes.Contains(type))
            return true;

        if (type.IsEnum)
            return true;

        var nullableUnderlying = Nullable.GetUnderlyingType(type);
        if (nullableUnderlying != null && nullableUnderlying.IsEnum)
            return true;

        return false;
    }
}