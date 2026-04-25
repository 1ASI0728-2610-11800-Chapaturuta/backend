namespace Frock_backend.Collections.Interfaces.REST.Resources;

public record CollectionItemResource(int Id, int FkIdCollection, int FkIdRoute, DateTime AddedAt);
