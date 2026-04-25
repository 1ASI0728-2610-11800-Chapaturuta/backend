namespace Frock_backend.Collections.Interfaces.REST.Resources;

public record CollectionResource(int Id, string Name, int FkIdUser, DateTime CreatedAt, int ItemCount);
