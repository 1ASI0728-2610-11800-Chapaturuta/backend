// Wired in AppDbContext by F4 agent
using Frock_backend.Driver.Domain.Model.Aggregates;
using Frock_backend.Driver.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frock_backend.Driver.Infrastructure.Persistence.EFC.Configuration;

public class TariffEntityConfiguration : IEntityTypeConfiguration<Tariff>
{
    public void Configure(EntityTypeBuilder<Tariff> builder)
    {
        builder.ToTable("tariffs");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(t => t.FkIdDriver).HasColumnName("fk_id_driver").IsRequired();
        builder.Property(t => t.BaseFare).HasColumnName("base_fare").HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(t => t.PricePerKm).HasColumnName("price_per_km").HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(t => t.PricePerMinute).HasColumnName("price_per_minute").HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(t => t.MinFare).HasColumnName("min_fare").HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(t => t.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();

        builder.Property(t => t.WeeklyAvailability)
            .HasColumnName("available_days")
            .HasConversion(
                v => v.ToCsv(),
                s => WeeklyAvailability.FromCsv(s))
            .HasMaxLength(200);

        builder.Property(t => t.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
    }
}
