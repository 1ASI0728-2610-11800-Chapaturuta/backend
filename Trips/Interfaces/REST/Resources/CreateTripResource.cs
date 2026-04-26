using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Trips.Interfaces.REST.Resources;

public record CreateTripResource(
    [property: SwaggerSchema("ID of the passenger user who is taking this trip")]
    int FkIdUser,
    [property: SwaggerSchema("ID of the driver assigned to this trip")]
    int FkIdDriver,
    [property: SwaggerSchema("ID of the route this trip follows")]
    int FkIdRoute,
    [property: SwaggerSchema("ID of the stop where the passenger boards the vehicle")]
    int FkIdOriginStop,
    [property: SwaggerSchema("ID of the stop where the passenger exits the vehicle")]
    int FkIdDestinationStop,
    [property: SwaggerSchema("Fare paid for this trip in Peruvian soles (S/.). Defaults to route price if not provided")]
    double? Price
);
