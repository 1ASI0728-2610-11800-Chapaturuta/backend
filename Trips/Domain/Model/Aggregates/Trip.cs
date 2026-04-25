namespace Frock_backend.Trips.Domain.Model.Aggregates;

public class Trip
{
    public int Id { get; }
    public int FkIdUser { get; set; }
    public int FkIdDriver { get; set; }
    public int FkIdRoute { get; set; }
    public int FkIdOriginStop { get; set; }
    public int FkIdDestinationStop { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double? Price { get; set; }
    public string Status { get; set; } = "Pending";

    protected Trip() { }

    public Trip(int fkIdUser, int fkIdDriver, int fkIdRoute, int fkIdOriginStop, int fkIdDestinationStop, double? price)
    {
        FkIdUser = fkIdUser;
        FkIdDriver = fkIdDriver;
        FkIdRoute = fkIdRoute;
        FkIdOriginStop = fkIdOriginStop;
        FkIdDestinationStop = fkIdDestinationStop;
        StartTime = DateTime.UtcNow;
        Price = price;
        Status = "InProgress";
    }
}
