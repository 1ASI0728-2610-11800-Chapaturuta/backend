// Wired in AppDbContext by F4 agent
using Frock_backend.Trips.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Frock_backend.Trips.Infrastructure.Persistence.EFC.Configuration;

public class ReservationEntityConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(r => r.FkIdUser).HasColumnName("fk_id_user").IsRequired();
        builder.Property(r => r.FkIdTrip).HasColumnName("fk_id_trip").IsRequired();
        builder.Property(r => r.DocumentType)
            .HasColumnName("document_type")
            .HasConversion<string>()
            .IsRequired();
        builder.Property(r => r.DocumentNumber).HasColumnName("document_number").HasMaxLength(20).IsRequired();
        builder.Property(r => r.Seats).HasColumnName("seats").IsRequired();
        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();
        builder.Property(r => r.FkIdPayment).HasColumnName("fk_id_payment").IsRequired(false);
        builder.Property(r => r.ReservedAt).HasColumnName("reserved_at").IsRequired();
        builder.Property(r => r.ConfirmedAt).HasColumnName("confirmed_at").IsRequired(false);

        builder.HasOne<Trip>()
            .WithMany()
            .HasForeignKey(r => r.FkIdTrip)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
