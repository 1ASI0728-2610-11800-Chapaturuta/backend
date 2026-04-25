namespace Frock_backend.Ratings.Domain.Model.Aggregates;

public class Rating
{
    public int Id { get; }
    public int FkIdUser { get; set; }
    public int FkIdDriver { get; set; }
    public int FkIdTrip { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    protected Rating()
    {
        Comment = string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    public Rating(int fkIdUser, int fkIdDriver, int fkIdTrip, int score, string? comment)
    {
        FkIdUser = fkIdUser;
        FkIdDriver = fkIdDriver;
        FkIdTrip = fkIdTrip;
        Score = score;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }
}
