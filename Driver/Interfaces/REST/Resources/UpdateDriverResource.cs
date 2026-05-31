using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Driver.Interfaces.REST.Resources;

public record UpdateDriverResource(
    [property: SwaggerSchema("Nuevos nombres del conductor")]
    string FirstName,
    [property: SwaggerSchema("Nuevos apellidos del conductor")]
    string LastName,
    [property: SwaggerSchema("Nuevo telefono de contacto")]
    string Phone,
    [property: SwaggerSchema("Nueva URL de la foto de perfil")]
    string PhotoUrl
);
