using Frock_backend.Collections.Domain.Model.Aggregates;
using Frock_backend.Collections.Interfaces.REST.Resources;

namespace Frock_backend.Collections.Interfaces.REST.Transform;

public static class CollectionResourceFromEntityAssembler
{
    public static CollectionResource ToResourceFromEntity(Collection entity) =>
        new CollectionResource(entity.Id, entity.Name, entity.FkIdUser, entity.CreatedAt, entity.Items?.Count ?? 0);
}
