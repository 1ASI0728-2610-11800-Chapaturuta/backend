// Wired in AppDbContext by F4 agent
using Frock_backend.Payments.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frock_backend.Payments.Infrastructure.Persistence.EFC.Configuration;

public class PaymentEntityConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(p => p.FkIdUser).HasColumnName("fk_id_user").IsRequired();
        builder.Property(p => p.Method)
            .HasColumnName("method")
            .HasConversion<string>()
            .IsRequired();
        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();
        builder.Property(p => p.ExternalReference).HasColumnName("external_reference").IsRequired(false);
        builder.Property(p => p.ReferenceType).HasColumnName("reference_type").IsRequired();
        builder.Property(p => p.ReferenceId).HasColumnName("reference_id").IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.ConfirmedAt).HasColumnName("confirmed_at").IsRequired(false);

        builder.OwnsOne(p => p.Amount, money =>
        {
            money.WithOwner().HasForeignKey("Id");
            money.Property<int>("Id").HasColumnName("id");
            money.Property(m => m.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        });
    }
}
