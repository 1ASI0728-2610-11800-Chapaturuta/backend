namespace Frock_backend.IAM.Domain.Model.Commands;

public record CreateDriverProfileCommand(
    int FkIdUser,
    string LicenseNumber,
    string VehiclePlate,
    string VehicleModel,
    int VehicleYear,
    int VehicleCapacity);
