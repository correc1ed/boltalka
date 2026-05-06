using boltalka.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database;

public class ServiceDbContext : DbContext
{
    public ServiceDbContext(DbContextOptions<ServiceDbContext> options) : base(options) { }

    public virtual DbSet<CallEntity> Calls => Set<CallEntity>();
    
    public virtual DbSet<ChatEntity> Chats => Set<ChatEntity>();
    
    public virtual DbSet<ChatMemberEntity> ChatMembers => Set<ChatMemberEntity>();
    
    public virtual DbSet<MediaEntity> Media => Set<MediaEntity>();
    
    public virtual DbSet<MessageEntity> Messages => Set<MessageEntity>();
    
    public virtual DbSet<MessageMediaEntity> MessagesMedia => Set<MessageMediaEntity>();
    
    public virtual DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("boltalka"); 
        
        base.OnModelCreating(modelBuilder);
        // Применяем конфигурации из сборки
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDbContext).Assembly);
    }
}
