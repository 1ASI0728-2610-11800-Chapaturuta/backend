// Wired in AppDbContext by F4 agent
using Frock_backend.Driver.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frock_backend.Driver.Infrastructure.Persistence.EFC.Configuration;

public class RouteDurationEntityConfiguration : IEntityTypeConfiguration<RouteDuration>
{
    public void Configure(EntityTypeBuilder<RouteDuration> builder)
    {
        builder.ToTable("route_durations");
        builder.HasKey(rd => rd.Id);

        builder.Property(rd => rd.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(rd => rd.FkIdTariff).HasColumnName("fk_id_tariff").IsRequired();
        builder.Property(rd => rd.FkIdRoute).HasColumnName("fk_id_route").IsRequired();
        builder.Property(rd => rd.EstimatedMinutes).HasColumnName("estimated_minutes").IsRequired();
    }
}
