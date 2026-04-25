namespace Frock_backend.Ratings.Interfaces.REST.Resources;

public record CreateRatingResource(int FkIdUser, int FkIdDriver, int FkIdTrip, int Score, string? Comment);
