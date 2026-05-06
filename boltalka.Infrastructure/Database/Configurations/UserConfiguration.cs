using boltalka.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace boltalka.Infrastructure.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");

        // Связь с аватаром (один-ко-многим, но фактически один-к-одному через nullable FK)
        builder.HasOne(u => u.Avatar)
            .WithMany()
            .HasForeignKey(u => u.AvatarMediaId)
            .OnDelete(DeleteBehavior.SetNull); // при удалении медиа не удаляем пользователя

        // При удалении пользователя не удаляем его сообщения и звонки (история должна остаться)
        builder.HasMany(u => u.Messages)
            .WithOne(m => m.Sender)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Calls)
            .WithOne(c => c.Initiator)
            .HasForeignKey(c => c.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Связь с членством в чатах: при удалении пользователя удаляем его членства
        builder.HasMany(u => u.ChatMembers)
            .WithOne(cm => cm.UserEntity)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(u => u.IsActive)
            .HasDefaultValue(true)
            .IsRequired();
    }
}