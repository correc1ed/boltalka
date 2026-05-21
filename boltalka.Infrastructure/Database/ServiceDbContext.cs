using boltalka.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace boltalka.Infrastructure.Database;

public class ServiceDbContext : DbContext
{
    public ServiceDbContext(DbContextOptions<ServiceDbContext> options) : base(options) { }

    /// <summary>
    /// Звонки.
    /// </summary>
    public virtual DbSet<CallEntity> Calls => Set<CallEntity>();
    
    /// <summary>
    /// Чаты.
    /// </summary>
    public virtual DbSet<ChatEntity> Chats => Set<ChatEntity>();
    
    /// <summary>
    /// Участники чата.
    /// </summary>
    public virtual DbSet<ChatMemberEntity> ChatMembers => Set<ChatMemberEntity>();
    
    /// <summary>
    /// Медиа.
    /// </summary>
    public virtual DbSet<MediaEntity> Media => Set<MediaEntity>();
    
    /// <summary>
    /// Сообщения.
    /// </summary>
    public virtual DbSet<MessageEntity> Messages => Set<MessageEntity>();
    
    /// <summary>
    /// Медиа сообщения.
    /// </summary>
    public virtual DbSet<MessageMediaEntity> MessagesMedia => Set<MessageMediaEntity>();
    
    /// <summary>
    /// Пользователи.
    /// </summary>
    public virtual DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("boltalka"); 
        
        base.OnModelCreating(modelBuilder);
        // Применяем конфигурации из сборки
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDbContext).Assembly);
    }
}
