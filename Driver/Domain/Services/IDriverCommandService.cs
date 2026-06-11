using Frock_backend.Driver.Domain.Model.Commands;
using DriverAggregate = Frock_backend.Driver.Domain.Model.Aggregates.Driver;

namespace Frock_backend.Driver.Domain.Services;

public interface IDriverCommandService
{
    Task<DriverAggregate?> Handle(CreateDriverCommand command);
    Task<DriverAggregate?> Handle(UpdateDriverCommand command);
    Task<DriverAggregate?> Handle(UpdateVehicleCommand command);
    Task<DriverAggregate?> Handle(ToggleAvailabilityCommand command);
    Task<DriverAggregate?> Handle(UpdateDriverPhotoCommand command);
    Task<bool> Handle(DeleteDriverCommand command);
}
