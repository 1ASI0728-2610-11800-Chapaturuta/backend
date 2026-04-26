using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Collections.Interfaces.REST.Resources;

public record UpdateCollectionResource(
    [property: SwaggerSchema("New display name for the collection")]
    string Name
);
