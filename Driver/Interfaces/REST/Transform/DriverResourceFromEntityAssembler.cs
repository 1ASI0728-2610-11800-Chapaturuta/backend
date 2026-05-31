using Frock_backend.Driver.Interfaces.REST.Resources;
using DriverAggregate = Frock_backend.Driver.Domain.Model.Aggregates.Driver;

namespace Frock_backend.Driver.Interfaces.REST.Transform;

public static class DriverResourceFromEntityAssembler
{
    public static DriverResource ToResourceFromEntity(DriverAggregate entity) =>
        new DriverResource(
            entity.Id,
            entity.FkIdUser,
            entity.FirstName,
            entity.LastName,
            entity.DocumentNumber,
            entity.Phone,
            entity.PhotoUrl,
            entity.LicenseNumber,
            entity.LicenseCategory,
            entity.Vehicle.Plate,
            entity.Vehicle.Brand,
            entity.Vehicle.Model,
            entity.Vehicle.Year,
            entity.Vehicle.Capacity,
            entity.Vehicle.Type,
            entity.IsAvailable,
            entity.CreatedAt,
            entity.UpdatedAt);
}
