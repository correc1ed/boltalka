using boltalka.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace boltalka.Infrastructure.Database.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<MessageEntity>
{
    public void Configure(EntityTypeBuilder<MessageEntity> builder)
    {
        builder.ToTable("Messages");

        // Кластеризованный индекс для быстрой загрузки истории чата (ChatId + SentAt)
        builder.HasIndex(m => new { m.ChatId, m.CreatedAt });
            //.UseClustering();

        // Дополнительный индекс по отправителю (для поиска своих сообщений)
        builder.HasIndex(m => m.SenderId);

        // Enum статуса храним строкой
        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Каскадные удаления описаны в ChatConfiguration и UserConfiguration, но на всякий случай уточним
        // (связь Chat -> Messages уже Cascade, Sender -> Messages Restrict)
    }
}