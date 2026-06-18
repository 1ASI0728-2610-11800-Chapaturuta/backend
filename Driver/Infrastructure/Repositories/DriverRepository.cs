using Frock_backend.Driver.Domain.Model.Aggregates;
using Frock_backend.Driver.Domain.Model.ValueObjects;
using Frock_backend.Driver.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using DriverAggregate = Frock_backend.Driver.Domain.Model.Aggregates.Driver;

namespace Frock_backend.Driver.Infrastructure.Repositories;

public class DriverRepository(AppDbContext context) : BaseRepository<DriverAggregate>(context), IDriverRepository
{
    public async Task<DriverAggregate?> FindByFkIdUserAsync(int fkIdUser)
    {
        return await Context.Set<DriverAggregate>()
            .FirstOrDefaultAsync(d => d.FkIdUser == fkIdUser && !d.IsDeleted);
    }

    public async Task<IEnumerable<DriverAggregate>> FindByIdsAsync(IEnumerable<int> ids)
    {
        var idSet = ids.ToHashSet();
        return await Context.Set<DriverAggregate>()
            .Where(d => idSet.Contains(d.Id) && !d.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<DriverAggregate>> FindByVehicleTypeAsync(VehicleType vehicleType)
    {
        return await Context.Set<DriverAggregate>()
            .Where(d => !d.IsDeleted && d.Vehicle.Type == vehicleType)
            .ToListAsync();
    }

    public async Task<IEnumerable<DriverAggregate>> FindAvailableByDayOfWeekAsync(DayOfWeek day)
    {
        // Join drivers with their active tariff, then filter by WeeklyAvailability in memory.
        // WeeklyAvailability is stored as a CSV string via HasConversion, so we materialize first.
        var candidates = await (
            from d in Context.Set<DriverAggregate>()
            join t in Context.Set<Tariff>() on d.Id equals t.FkIdDriver
            where !d.IsDeleted && d.IsAvailable && t.IsActive
            select new { Driver = d, Tariff = t }
        ).ToListAsync();

        return candidates
            .Where(x => x.Tariff.WeeklyAvailability.IsAvailableOn(day))
            .Select(x => x.Driver)
            .Distinct()
            .ToList();
    }
}
