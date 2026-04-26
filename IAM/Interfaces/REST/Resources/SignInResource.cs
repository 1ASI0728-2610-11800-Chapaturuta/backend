using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.IAM.Interfaces.REST.Resources;

public record SignInResource(
    [property: SwaggerSchema("Email address of the registered user", Format = "email")]
    string Email,
    [property: SwaggerSchema("Account password (min 8 chars, must include uppercase letter and number)")]
    string Password
);