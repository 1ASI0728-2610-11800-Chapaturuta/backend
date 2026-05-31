using Frock_backend.Driver.Domain.Model.Queries;
using DriverAggregate = Frock_backend.Driver.Domain.Model.Aggregates.Driver;

namespace Frock_backend.Driver.Domain.Services;

public interface IDriverQueryService
{
    Task<IEnumerable<DriverAggregate>> Handle(GetAllDriversQuery query);
    Task<DriverAggregate?> Handle(GetDriverByIdQuery query);
    Task<DriverAggregate?> Handle(GetDriverByFkIdUserQuery query);
    Task<IEnumerable<DriverAggregate>> Handle(GetDriversByVehicleTypeQuery query);
    Task<IEnumerable<DriverAggregate>> Handle(GetAvailableDriversByDayOfWeekQuery query);
}
