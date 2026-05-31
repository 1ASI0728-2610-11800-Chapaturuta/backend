// Wired in AppDbContext by F4 agent
using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frock_backend.Subscriptions.Infrastructure.Persistence.EFC.Configuration;

public class SubscriptionEntityConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(s => s.FkIdUser).HasColumnName("fk_id_user").IsRequired();
        builder.Property(s => s.FkIdPlan).HasColumnName("fk_id_plan").IsRequired();
        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(s => s.StartsAt).HasColumnName("starts_at").IsRequired();
        builder.Property(s => s.EndsAt).HasColumnName("ends_at").IsRequired();
        builder.Property(s => s.AutoRenew).HasColumnName("auto_renew").IsRequired();
        builder.Property(s => s.FkIdPayment).HasColumnName("fk_id_payment").IsRequired(false);
        builder.Property(s => s.DiscoveryUsageInCycle).HasColumnName("discovery_usage_in_cycle").IsRequired();
    }
}
