using Frock_backend.IAM.Domain.Model.ValueObjects;

namespace Frock_backend.IAM.Interfaces.REST.Resources;

public record UserResource(int Id, string Username, Role Role);