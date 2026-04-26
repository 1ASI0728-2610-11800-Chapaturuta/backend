using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.routes.Interface.REST.Resources;

public record CoordinateResource(
    [property: SwaggerSchema("Geographic latitude in decimal degrees (negative = south)")]
    double Latitude,
    [property: SwaggerSchema("Geographic longitude in decimal degrees (negative = west)")]
    double Longitude
);

public record RoutePreviewResource(
    [property: SwaggerSchema("Ordered list of GPS coordinates that trace the route path")]
    List<CoordinateResource> Coordinates
);
