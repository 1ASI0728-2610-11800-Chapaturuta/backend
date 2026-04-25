namespace Frock_backend.IAM.Interfaces.REST.Resources;

public record DriverProfileResource(
    int Id,
    int FkIdUser,
    string LicenseNumber,
    string VehiclePlate,
    string VehicleModel,
    int VehicleYear,
    int VehicleCapacity,
    bool IsAvailable);
