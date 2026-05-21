using boltalka.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace boltalka.Infrastructure.Database.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<ChatEntity>
{
    public void Configure(EntityTypeBuilder<ChatEntity> builder)
    {
        builder.ToTable("Chats");

        // Enum храним строкой для читаемости (опционально)
        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        // При удалении чата удаляем все его сообщения и звонки (Cascade)
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Chat)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Calls)
            .WithOne(call => call.Chat)
            .HasForeignKey(call => call.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        // При удалении чата удаляем все записи о членстве
        builder.HasMany(c => c.Members)
            .WithOne(cm => cm.Chat)
            .HasForeignKey(cm => cm.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}