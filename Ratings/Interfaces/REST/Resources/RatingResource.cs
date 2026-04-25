namespace Frock_backend.Ratings.Interfaces.REST.Resources;

public record RatingResource(int Id, int FkIdUser, int FkIdDriver, int FkIdTrip, int Score, string? Comment, DateTime CreatedAt);
