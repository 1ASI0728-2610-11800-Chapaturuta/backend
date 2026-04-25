namespace Frock_backend.Ratings.Domain.Model.Commands;

public record CreateRatingCommand(int FkIdUser, int FkIdDriver, int FkIdTrip, int Score, string? Comment);
