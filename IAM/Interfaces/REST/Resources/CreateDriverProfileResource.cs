using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.IAM.Interfaces.REST.Resources;

public record CreateDriverProfileResource(
    [property: SwaggerSchema("ID of the user account that will be linked to this driver profile")]
    int FkIdUser,
    [property: SwaggerSchema("Official driver's license number issued by the Peruvian transport authority")]
    string LicenseNumber,
    [property: SwaggerSchema("Vehicle license plate in Peruvian format, e.g. ABC-123")]
    string VehiclePlate,
    [property: SwaggerSchema("Make and model of the vehicle, e.g. Toyota Coaster")]
    string VehicleModel,
    [property: SwaggerSchema("Year the vehicle was manufactured")]
    int VehicleYear,
    [property: SwaggerSchema("Maximum passenger capacity of the vehicle")]
    int VehicleCapacity
);
