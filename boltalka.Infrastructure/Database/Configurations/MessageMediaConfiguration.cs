using boltalka.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace boltalka.Infrastructure.Database.Configurations;

public class MessageMediaConfiguration : IEntityTypeConfiguration<MessageMediaEntity>
{
    public void Configure(EntityTypeBuilder<MessageMediaEntity> builder)
    {
        builder.ToTable("MessageMedia");

        // Составной первичный ключ задан атрибутом, но можно подчеркнуть здесь
        builder.HasKey(mm => new { mm.MessageId, mm.MediaId });

        // Связь с сообщением: при удалении сообщения удаляем связь
        builder.HasOne(mm => mm.Message)
            .WithMany(m => m.MessageMediaLinks)
            .HasForeignKey(mm => mm.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связь с медиа: при удалении медиа не удаляем сообщение, только связь (Restrict)
        builder.HasOne(mm => mm.Media)
            .WithMany()
            .HasForeignKey(mm => mm.MediaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}