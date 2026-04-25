namespace Frock_backend.IAM.Interfaces.REST.Resources;

public record CreateDriverProfileResource(
    int FkIdUser,
    string LicenseNumber,
    string VehiclePlate,
    string VehicleModel,
    int VehicleYear,
    int VehicleCapacity);
