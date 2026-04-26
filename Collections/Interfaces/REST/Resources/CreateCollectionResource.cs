using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Collections.Interfaces.REST.Resources;

public record CreateCollectionResource(
    [property: SwaggerSchema("Display name for the new collection, e.g. Mis rutas favoritas")]
    string Name,
    [property: SwaggerSchema("ID of the user who owns this collection")]
    int FkIdUser
);
