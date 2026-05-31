using Frock_backend.Driver.Domain.Model.ValueObjects;

namespace Frock_backend.Driver.Interfaces.REST.Resources;

public record DriverResource(
    int Id,
    int FkIdUser,
    string FirstName,
    string LastName,
    string DocumentNumber,
    string Phone,
    string PhotoUrl,
    string LicenseNumber,
    LicenseCategory LicenseCategory,
    string VehiclePlate,
    string VehicleBrand,
    string VehicleModel,
    int VehicleYear,
    int VehicleCapacity,
    VehicleType VehicleType,
    bool IsAvailable,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
