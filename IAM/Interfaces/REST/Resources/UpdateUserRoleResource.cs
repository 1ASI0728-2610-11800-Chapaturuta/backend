using Frock_backend.IAM.Domain.Model.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.IAM.Interfaces.REST.Resources;

public record UpdateUserRoleResource(
    [property: SwaggerSchema("New role to assign: 0 = Traveller, 1 = TransportManager, 2 = Driver, 3 = Admin")]
    Role Role
);
