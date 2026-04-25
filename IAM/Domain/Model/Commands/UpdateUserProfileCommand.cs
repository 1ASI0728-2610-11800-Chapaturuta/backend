namespace Frock_backend.IAM.Domain.Model.Commands;

public record UpdateUserProfileCommand(int Id, string Username, string Email);
