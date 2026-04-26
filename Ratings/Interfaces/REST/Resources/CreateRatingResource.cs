using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Ratings.Interfaces.REST.Resources;

public record CreateRatingResource(
    [property: SwaggerSchema("ID of the user who is submitting the rating")]
    int FkIdUser,
    [property: SwaggerSchema("ID of the driver being rated")]
    int FkIdDriver,
    [property: SwaggerSchema("ID of the completed trip this rating refers to")]
    int FkIdTrip,
    [property: SwaggerSchema("Rating score from 1 (worst) to 5 (best)")]
    int Score,
    [property: SwaggerSchema("Optional written review about the trip or driver")]
    string? Comment
);
