using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.IAM.Interfaces.REST.Resources;

public record UpdateUserProfileResource(
    [property: SwaggerSchema("New display name for the user account")]
    string Username,
    [property: SwaggerSchema("New email address for the user account", Format = "email")]
    string Email
);
