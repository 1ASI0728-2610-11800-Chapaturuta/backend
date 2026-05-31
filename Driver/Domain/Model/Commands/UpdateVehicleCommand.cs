using Frock_backend.Driver.Domain.Model.ValueObjects;

namespace Frock_backend.Driver.Domain.Model.Commands;

public record UpdateVehicleCommand(
    int DriverId,
    string Plate,
    string Brand,
    string Model,
    int Year,
    int Capacity,
    VehicleType VehicleType
);
