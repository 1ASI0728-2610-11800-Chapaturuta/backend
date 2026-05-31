using Frock_backend.Driver.Domain.Model.Commands;
using Frock_backend.Driver.Interfaces.REST.Resources;

namespace Frock_backend.Driver.Interfaces.REST.Transform;

public static class CreateDriverCommandFromResourceAssembler
{
    public static CreateDriverCommand ToCommandFromResource(CreateDriverResource resource) =>
        new CreateDriverCommand(
            resource.FkIdUser,
            resource.FirstName,
            resource.LastName,
            resource.DocumentNumber,
            resource.Phone,
            resource.PhotoUrl,
            resource.LicenseNumber,
            resource.LicenseCategory,
            resource.VehiclePlate,
            resource.VehicleBrand,
            resource.VehicleModel,
            resource.VehicleYear,
            resource.VehicleCapacity,
            resource.VehicleType);
}
