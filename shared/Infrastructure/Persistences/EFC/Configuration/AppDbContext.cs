using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Model.Entities;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration.Extensions;
using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Model.Aggregates.Geographic;
using Frock_backend.transport_Company.Domain.Model.Aggregates;
using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.Ratings.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Collections.Domain.Model.Aggregates;
using Frock_backend.Notifications.Domain.Model.Aggregates;

using Microsoft.EntityFrameworkCore;


namespace Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.AddCreatedUpdatedInterceptor();
            base.OnConfiguring(builder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // IAM Context
            builder.Entity<User>().HasKey(u => u.Id);
            builder.Entity<User>().Property(u => u.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<User>().Property(u => u.Username).IsRequired();
            builder.Entity<User>().Property(u => u.PasswordHash).IsRequired();
            builder.Entity<User>().Property(u => u.Role).HasConversion<string>().IsRequired();

            // DRIVER PROFILE
            builder.Entity<DriverProfile>().HasKey(dp => dp.Id);
            builder.Entity<DriverProfile>().Property(dp => dp.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<DriverProfile>().Property(dp => dp.LicenseNumber).IsRequired();
            builder.Entity<DriverProfile>().Property(dp => dp.VehiclePlate).IsRequired();
            builder.Entity<DriverProfile>().Property(dp => dp.VehicleModel).IsRequired();
            builder.Entity<DriverProfile>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<DriverProfile>(dp => dp.FkIdUser)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // COMPANY
            builder.Entity<Company>().HasKey(f => f.Id);
            builder.Entity<Company>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<Company>().Property(f => f.Name).IsRequired();
            builder.Entity<Company>().Property(f => f.LogoUrl).IsRequired();
            builder.Entity<Company>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Company>(c => c.FkIdUser)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // REGION
            builder.Entity<Region>().HasKey(f => f.Id);
            builder.Entity<Region>().Property(f => f.Id).IsRequired();
            builder.Entity<Region>().Property(f => f.Name).IsRequired();

            // PROVINCE
            builder.Entity<Province>().HasKey(f => f.Id);
            builder.Entity<Province>().Property(f => f.Id).IsRequired();
            builder.Entity<Province>().Property(f => f.Name).IsRequired();
            builder.Entity<Province>()
                .HasOne<Region>()
                .WithMany()
                .HasForeignKey(p => p.FkIdRegion)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // DISTRICT
            builder.Entity<District>().HasKey(f => f.Id);
            builder.Entity<District>().Property(f => f.Id).IsRequired();
            builder.Entity<District>().Property(f => f.Name).IsRequired();
            builder.Entity<District>()
                .HasOne<Province>()
                .WithMany()
                .HasForeignKey(d => d.FkIdProvince)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // STOP
            builder.Entity<Stop>().HasKey(f => f.Id);
            builder.Entity<Stop>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<Stop>().Property(f => f.Name).IsRequired();
            builder.Entity<Stop>().Property(f => f.GoogleMapsUrl);
            builder.Entity<Stop>().Property(f => f.ImageUrl);
            builder.Entity<Stop>().Property(f => f.Phone).IsRequired();
            builder.Entity<Stop>().Property(f => f.Address).IsRequired();
            builder.Entity<Stop>().Property(f => f.Reference).IsRequired();
            builder.Entity<Stop>().Property(f => f.Latitude);
            builder.Entity<Stop>().Property(f => f.Longitude);
            builder.Entity<Stop>()
                .HasOne<Company>()
                .WithMany()
                .HasForeignKey(l => l.FkIdCompany)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Stop>()
                .HasOne<District>()
                .WithMany()
                .HasForeignKey(f => f.FkIdDistrict)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // ROUTE
            builder.Entity<RouteAggregate>(b =>
            {
                b.ToTable("Routes");
                b.HasKey(r => r.Id);
                b.Property(r => r.Price).IsRequired();
                b.Property(r => r.Duration).IsRequired();
                b.Property(r => r.Frequency).IsRequired();
                b.Property(r => r.IsActive).HasDefaultValue(true);
                b.Property(r => r.Status).HasDefaultValue("Active").HasMaxLength(20);
                b.Property(r => r.DistanceMeters).HasColumnType("decimal(12,2)");
                b.Property(r => r.DurationSeconds);
                b.Property(r => r.Geometry).HasColumnType("TEXT");
                b.HasMany(r => r.Schedules)
                 .WithOne()
                 .HasForeignKey(s => s.RouteId)
                 .OnDelete(DeleteBehavior.Cascade);
                builder.Entity<RoutesStops>(b =>
                {
                    b.ToTable("RouteStops");
                    b.HasKey(rs => rs.Id);
                    b.HasOne(rs => rs.Route)
                     .WithMany(r => r.Stops)
                     .HasForeignKey(rs => rs.FKRouteId);
                    b.HasOne(rs => rs.Stop)
                     .WithMany()
                     .HasForeignKey(rs => rs.FkStopId)
                     .OnDelete(DeleteBehavior.Restrict);
                });
            });

            // SCHEDULE
            builder.Entity<Schedule>(b =>
            {
                b.ToTable("Schedules");
                b.HasKey(s => s.Id);
                b.Property(s => s.StartTime).IsRequired();
                b.Property(s => s.EndTime).IsRequired();
                b.Property(s => s.DayOfWeek).HasMaxLength(10);
            });

            // RATING
            builder.Entity<Rating>(b =>
            {
                b.ToTable("Ratings");
                b.HasKey(r => r.Id);
                b.Property(r => r.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(r => r.Score).IsRequired();
                b.Property(r => r.Comment).HasMaxLength(500);
                b.Property(r => r.CreatedAt).IsRequired();
                b.HasOne<User>().WithMany().HasForeignKey(r => r.FkIdUser).OnDelete(DeleteBehavior.Restrict);
                b.HasOne<User>().WithMany().HasForeignKey(r => r.FkIdDriver).OnDelete(DeleteBehavior.Restrict);
            });

            // TRIP
            builder.Entity<Trip>(b =>
            {
                b.ToTable("Trips");
                b.HasKey(t => t.Id);
                b.Property(t => t.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(t => t.StartTime).IsRequired();
                b.Property(t => t.Status).HasMaxLength(20).HasDefaultValue("Pending");
                b.HasOne<User>().WithMany().HasForeignKey(t => t.FkIdUser).OnDelete(DeleteBehavior.Restrict);
                b.HasOne<User>().WithMany().HasForeignKey(t => t.FkIdDriver).OnDelete(DeleteBehavior.Restrict);
                b.HasOne<RouteAggregate>().WithMany().HasForeignKey(t => t.FkIdRoute).OnDelete(DeleteBehavior.Restrict);
                b.HasOne<Stop>().WithMany().HasForeignKey(t => t.FkIdOriginStop).OnDelete(DeleteBehavior.Restrict);
                b.HasOne<Stop>().WithMany().HasForeignKey(t => t.FkIdDestinationStop).OnDelete(DeleteBehavior.Restrict);
            });

            // COLLECTION
            builder.Entity<Collection>(b =>
            {
                b.ToTable("Collections");
                b.HasKey(c => c.Id);
                b.Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(c => c.Name).IsRequired().HasMaxLength(100);
                b.Property(c => c.CreatedAt).IsRequired();
                b.HasOne<User>().WithMany().HasForeignKey(c => c.FkIdUser).OnDelete(DeleteBehavior.Cascade);
                b.HasMany(c => c.Items).WithOne().HasForeignKey(ci => ci.FkIdCollection).OnDelete(DeleteBehavior.Cascade);
            });

            // COLLECTION ITEM
            builder.Entity<CollectionItem>(b =>
            {
                b.ToTable("CollectionItems");
                b.HasKey(ci => ci.Id);
                b.Property(ci => ci.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(ci => ci.AddedAt).IsRequired();
                b.HasOne<RouteAggregate>().WithMany().HasForeignKey(ci => ci.FkIdRoute).OnDelete(DeleteBehavior.Cascade);
            });

            // NOTIFICATION
            builder.Entity<Notification>(b =>
            {
                b.ToTable("Notifications");
                b.HasKey(n => n.Id);
                b.Property(n => n.Id).IsRequired().ValueGeneratedOnAdd();
                b.Property(n => n.Title).IsRequired().HasMaxLength(200);
                b.Property(n => n.Message).IsRequired().HasMaxLength(1000);
                b.Property(n => n.Type).HasMaxLength(20).HasDefaultValue("Info");
                b.Property(n => n.IsRead).HasDefaultValue(false);
                b.Property(n => n.CreatedAt).IsRequired();
                b.HasOne<User>().WithMany().HasForeignKey(n => n.FkIdUser).OnDelete(DeleteBehavior.Cascade);
            });

            builder.UseSnakeCaseNamingConvention();
        }
    }
}
