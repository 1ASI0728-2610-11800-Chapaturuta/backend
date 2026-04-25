using Frock_backend.Ratings.Domain.Model.Aggregates;
using Frock_backend.Ratings.Interfaces.REST.Resources;

namespace Frock_backend.Ratings.Interfaces.REST.Transform;

public static class RatingResourceFromEntityAssembler
{
    public static RatingResource ToResourceFromEntity(Rating entity) =>
        new RatingResource(entity.Id, entity.FkIdUser, entity.FkIdDriver, entity.FkIdTrip, entity.Score, entity.Comment, entity.CreatedAt);
}
