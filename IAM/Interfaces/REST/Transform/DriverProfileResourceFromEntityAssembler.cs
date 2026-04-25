using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.IAM.Interfaces.REST.Resources;

namespace Frock_backend.IAM.Interfaces.REST.Transform;

public static class DriverProfileResourceFromEntityAssembler
{
    public static DriverProfileResource ToResourceFromEntity(DriverProfile entity)
    {
        return new DriverProfileResource(
            entity.Id,
            entity.FkIdUser,
            entity.LicenseNumber,
            entity.VehiclePlate,
            entity.VehicleModel,
            entity.VehicleYear,
            entity.VehicleCapacity,
            entity.IsAvailable);
    }
}
