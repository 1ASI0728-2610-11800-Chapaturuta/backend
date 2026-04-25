using Frock_backend.IAM.Domain.Model.ValueObjects;

namespace Frock_backend.IAM.Interfaces.REST.Resources;

public record AuthenticatedUserResource(int Id, string Username, Role Role, string Token);