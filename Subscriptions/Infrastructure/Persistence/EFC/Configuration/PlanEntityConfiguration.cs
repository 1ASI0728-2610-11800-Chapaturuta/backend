// Wired in AppDbContext by F4 agent
using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frock_backend.Subscriptions.Infrastructure.Persistence.EFC.Configuration;

public class PlanEntityConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("plans");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(p => p.PlanType)
            .HasColumnName("plan_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(p => p.TargetRole)
            .HasColumnName("target_role")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(p => p.Price).HasColumnName("price").HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(p => p.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(p => p.BillingCycle)
            .HasColumnName("billing_cycle")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(p => p.Benefits).HasColumnName("benefits").HasMaxLength(1000).IsRequired();
        builder.Property(p => p.DiscoveryQuota).HasColumnName("discovery_quota").IsRequired(false);
        builder.Property(p => p.IsActive).HasColumnName("is_active").IsRequired();
    }
}
