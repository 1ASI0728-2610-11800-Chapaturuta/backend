using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.stops.Interfaces.REST.Resources
{
    public record UpdateStopResource(
        [property: SwaggerSchema("ID of the stop to update")]
        int Id,
        [property: SwaggerSchema("Human-readable name of the bus stop, e.g. Paradero Miraflores")]
        string Name,
        [property: SwaggerSchema("Google Maps URL pointing to the stop's location (optional)")]
        string GoogleMapsUrl,
        [property: SwaggerSchema("Public URL of the stop's photo image")]
        string ImageUrl,
        [property: SwaggerSchema("Contact phone number for the stop, e.g. 01-4441234")]
        string Phone,
        [property: SwaggerSchema("ID of the transport company that owns this stop")]
        int FkIdCompany,
        [property: SwaggerSchema("Street address of the bus stop, e.g. Av. Larco 123")]
        string Address,
        [property: SwaggerSchema("Nearby landmark or additional directions, e.g. Frente al parque")]
        string Reference,
        [property: SwaggerSchema("ID of the district where this stop is located")]
        int FkIdDistrict,
        [property: SwaggerSchema("Decimal latitude coordinate (negative = south hemisphere)")]
        double? Latitude = null,
        [property: SwaggerSchema("Decimal longitude coordinate (negative = west hemisphere)")]
        double? Longitude = null
    );
}