using Frock_backend.Driver.Domain.Model.ValueObjects;
using Frock_backend.shared.Domain.Repositories;
using DriverAggregate = Frock_backend.Driver.Domain.Model.Aggregates.Driver;

namespace Frock_backend.Driver.Domain.Repositories;

public interface IDriverRepository : IBaseRepository<DriverAggregate>
{
    Task<DriverAggregate?> FindByFkIdUserAsync(int fkIdUser);
    Task<IEnumerable<DriverAggregate>> FindByVehicleTypeAsync(VehicleType vehicleType);
    Task<IEnumerable<DriverAggregate>> FindAvailableByDayOfWeekAsync(DayOfWeek day);
}
