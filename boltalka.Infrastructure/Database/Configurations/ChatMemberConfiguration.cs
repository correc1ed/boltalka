using boltalka.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace boltalka.Infrastructure.Database.Configurations;

public class ChatMemberConfiguration : IEntityTypeConfiguration<ChatMemberEntity>
{
    public void Configure(EntityTypeBuilder<ChatMemberEntity> builder)
    {
        builder.ToTable("ChatMembers");

        // Составной первичный ключ уже задан атрибутом [PrimaryKey], но для кластеризации нужно явно указать индекс
        // Кластеризованный индекс по (ChatId, UserId) для быстрого получения участников чата
        builder.HasIndex(cm => new { cm.ChatId, cm.UserId });
            //.UseClustering();

        // Роль храним строкой
        builder.Property(cm => cm.Role)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}