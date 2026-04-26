using Frock_backend.IAM.Domain.Model.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.IAM.Interfaces.REST.Resources;


public class SignUpResource
{
    [SwaggerSchema("Unique display name for the user account")]
    public string Username { get; set; } = string.Empty;

    [SwaggerSchema("Email address used for login and notifications", Format = "email")]
    public string Email { get; set; } = string.Empty;

    [SwaggerSchema("Account password (min 8 chars, must include uppercase letter and number)")]
    public string Password { get; set; } = string.Empty;

    [SwaggerSchema("User role: 0 = Traveller, 1 = TransportManager, 2 = Driver, 3 = Admin")]
    public Role Role { get; set; }
}
