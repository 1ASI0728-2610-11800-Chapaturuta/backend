// Wired in AppDbContext by F4 agent
using Frock_backend.Payments.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frock_backend.Payments.Infrastructure.Persistence.EFC.Configuration;

public class RefundEntityConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("refunds");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(r => r.FkIdPayment).HasColumnName("fk_id_payment").IsRequired();
        builder.Property(r => r.Reason).HasColumnName("reason").HasMaxLength(500).IsRequired();
        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(r => r.ConfirmedAt).HasColumnName("confirmed_at").IsRequired(false);

        builder.OwnsOne(r => r.Amount, money =>
        {
            money.WithOwner().HasForeignKey("Id");
            money.Property<int>("Id").HasColumnName("id");
            money.Property(m => m.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        });

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(r => r.FkIdPayment)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
