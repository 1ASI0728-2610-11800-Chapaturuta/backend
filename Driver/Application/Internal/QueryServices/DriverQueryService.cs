using Frock_backend.Driver.Domain.Model.Queries;
using Frock_backend.Driver.Domain.Repositories;
using Frock_backend.Driver.Domain.Services;
using DriverAggregate = Frock_backend.Driver.Domain.Model.Aggregates.Driver;

namespace Frock_backend.Driver.Application.Internal.QueryServices;

public class DriverQueryService(IDriverRepository driverRepository) : IDriverQueryService
{
    public async Task<IEnumerable<DriverAggregate>> Handle(GetAllDriversQuery query)
    {
        return await driverRepository.ListAsync();
    }

    public async Task<DriverAggregate?> Handle(GetDriverByIdQuery query)
    {
        return await driverRepository.FindByIdAsync(query.Id);
    }

    public async Task<DriverAggregate?> Handle(GetDriverByFkIdUserQuery query)
    {
        return await driverRepository.FindByFkIdUserAsync(query.FkIdUser);
    }

    public async Task<IEnumerable<DriverAggregate>> Handle(GetDriversByVehicleTypeQuery query)
    {
        return await driverRepository.FindByVehicleTypeAsync(query.VehicleType);
    }

    public async Task<IEnumerable<DriverAggregate>> Handle(GetAvailableDriversByDayOfWeekQuery query)
    {
        return await driverRepository.FindAvailableByDayOfWeekAsync(query.Day);
    }
}
