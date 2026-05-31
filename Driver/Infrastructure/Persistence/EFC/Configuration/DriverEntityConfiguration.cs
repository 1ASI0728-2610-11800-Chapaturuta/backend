// Wired in AppDbContext by F4 agent
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DriverAggregate = Frock_backend.Driver.Domain.Model.Aggregates.Driver;

namespace Frock_backend.Driver.Infrastructure.Persistence.EFC.Configuration;

public class DriverEntityConfiguration : IEntityTypeConfiguration<DriverAggregate>
{
    public void Configure(EntityTypeBuilder<DriverAggregate> builder)
    {
        builder.ToTable("drivers");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(d => d.FkIdUser).HasColumnName("fk_id_user").IsRequired();
        builder.Property(d => d.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(80);
        builder.Property(d => d.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(80);
        builder.Property(d => d.DocumentNumber).HasColumnName("document_number").IsRequired().HasMaxLength(20);
        builder.Property(d => d.Phone).HasColumnName("phone").IsRequired().HasMaxLength(20);
        builder.Property(d => d.PhotoUrl).HasColumnName("photo_url").HasMaxLength(500);
        builder.Property(d => d.LicenseNumber).HasColumnName("license_number").IsRequired().HasMaxLength(20);
        builder.Property(d => d.LicenseCategory)
            .HasColumnName("license_category")
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(d => d.IsAvailable).HasColumnName("is_available").HasDefaultValue(true);
        builder.Property(d => d.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at").IsRequired(false);

        builder.OwnsOne(d => d.Vehicle, vehicle =>
        {
            vehicle.WithOwner().HasForeignKey("Id");
            vehicle.Property<int>("Id").HasColumnName("id");
            vehicle.Property(v => v.Plate).HasColumnName("vehicle_plate").IsRequired().HasMaxLength(10);
            vehicle.Property(v => v.Brand).HasColumnName("vehicle_brand").HasMaxLength(50);
            vehicle.Property(v => v.Model).HasColumnName("vehicle_model").HasMaxLength(50);
            vehicle.Property(v => v.Year).HasColumnName("vehicle_year");
            vehicle.Property(v => v.Capacity).HasColumnName("vehicle_capacity");
            vehicle.Property(v => v.Type)
                .HasColumnName("vehicle_type")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
        });
    }
}
