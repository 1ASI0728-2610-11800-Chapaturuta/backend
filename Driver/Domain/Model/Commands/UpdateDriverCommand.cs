namespace Frock_backend.Driver.Domain.Model.Commands;

public record UpdateDriverCommand(int Id, string FirstName, string LastName, string Phone, string PhotoUrl);
