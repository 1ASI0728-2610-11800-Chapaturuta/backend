using Frock_backend.Trips.Domain.Model.ValueObjects;

namespace Frock_backend.Trips.Domain.Model.Aggregates;

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
    public TripStatus Status { get; set; } = TripStatus.Pending;
    public int AvailableSeats { get; private set; }

    protected Trip() { }

    public Trip(int fkIdUser, int? fkIdDriver, int fkIdRoute, int fkIdOriginStop, int fkIdDestinationStop, decimal? price, int availableSeats = 0, DateTime? startTime = null)
    {
        FkIdUser = fkIdUser;
        FkIdDriver = fkIdDriver;
        FkIdRoute = fkIdRoute;
        FkIdOriginStop = fkIdOriginStop;
        FkIdDestinationStop = fkIdDestinationStop;
        StartTime = startTime ?? DateTime.UtcNow;
        Price = price;
        Status = TripStatus.Pending;
        AvailableSeats = availableSeats;
    }

    public void AssignDriver(int driverId)
    {
        if (Status != TripStatus.Pending)
            throw new InvalidOperationException($"Cannot assign a driver to a trip with status {Status}");
        if (FkIdDriver is not null)
            throw new InvalidOperationException("Trip already has a driver assigned");
        FkIdDriver = driverId;
    }

    public void Start()
    {
        Status = TripStatus.InProgress;
    }

    public void Complete()
    {
        Status = TripStatus.Completed;
        EndTime = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = TripStatus.Cancelled;
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
