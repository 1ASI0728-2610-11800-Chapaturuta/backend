using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.transport_Company.Interfaces.REST.Resources
{
    public record UpdateCompanyResource(
        [property: SwaggerSchema("ID of the transport company to update")]
        int Id,
        [property: SwaggerSchema("Official registered name of the transport company")]
        string Name,
        [property: SwaggerSchema("Public URL of the company's logo image")]
        string LogoUrl,
        [property: SwaggerSchema("ID of the user account that manages this company")]
        int FkIdUser,
        [property: SwaggerSchema("RUC (tax id) of the company")]
        string? Ruc,
        [property: SwaggerSchema("Contact phone")]
        string? Phone,
        [property: SwaggerSchema("Contact email")]
        string? Email,
        [property: SwaggerSchema("Postal address")]
        string? Address,
        [property: SwaggerSchema("Free-text description")]
        string? Description
    );
}
