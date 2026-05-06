using boltalka.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace boltalka.Infrastructure.Database.Configurations;

public class CallConfiguration : IEntityTypeConfiguration<CallEntity>
{
    public void Configure(EntityTypeBuilder<CallEntity> builder)
    {
        builder.ToTable("Calls");

        // Кластеризованный индекс для хронологии звонков в чате
        builder.HasIndex(c => new { c.ChatId, c.StartedAt });
            //.UseClustering();

        // Enum'ы храним строкой
        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}