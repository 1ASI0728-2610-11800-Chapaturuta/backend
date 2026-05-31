using Frock_backend.Driver.Domain.Model.ValueObjects;

namespace Frock_backend.Driver.Domain.Model.Commands;

public record CreateDriverCommand(
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
    VehicleType VehicleType
);
