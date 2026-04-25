namespace Frock_backend.Collections.Domain.Model.Commands;

public record CreateCollectionCommand(string Name, int FkIdUser);
