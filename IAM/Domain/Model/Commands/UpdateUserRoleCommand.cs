using Frock_backend.IAM.Domain.Model.ValueObjects;

namespace Frock_backend.IAM.Domain.Model.Commands;

public record UpdateUserRoleCommand(int Id, Role Role);
