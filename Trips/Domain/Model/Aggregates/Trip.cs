namespace Frock_backend.Trips.Domain.Model.Aggregates;

// available_seats column will be added by F4 in TripEntityConfiguration (or AppDbContext Trip block)
public class Trip
{
    public int Id { get; }
    public int FkIdUser { get; set; }
    public int? FkIdDriver { get; set; }
    public int FkIdRoute { get; set; }
    public int FkIdOriginStop { get; set; }
    public int FkIdDestinationStop { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? Price { get; set; }
    public string Status { get; set; } = "Pending";
    public int AvailableSeats { get; private set; }

    protected Trip() { }

    public Trip(int fkIdUser, int? fkIdDriver, int fkIdRoute, int fkIdOriginStop, int fkIdDestinationStop, decimal? price, int availableSeats = 0)
    {
        FkIdUser = fkIdUser;
        FkIdDriver = fkIdDriver;
        FkIdRoute = fkIdRoute;
        FkIdOriginStop = fkIdOriginStop;
        FkIdDestinationStop = fkIdDestinationStop;
        StartTime = DateTime.UtcNow;
        Price = price;
        Status = "InProgress";
        AvailableSeats = availableSeats;
    }

    public void ReserveSeats(int seats)
    {
        if (seats <= 0 || AvailableSeats < seats)
            throw new InvalidOperationException("Not enough available seats");
        AvailableSeats -= seats;
    }

    public void ReleaseSeats(int seats)
    {
        if (seats <= 0)
            throw new InvalidOperationException("Seats to release must be positive");
        AvailableSeats += seats;
    }
}
