namespace Frock_backend.Driver.Domain.Model.Aggregates;

public class RouteDuration
{
    public int Id { get; }
    public int FkIdTariff { get; set; }
    public int FkIdRoute { get; set; }
    public int EstimatedMinutes { get; set; }

    protected RouteDuration() { }

    public RouteDuration(int fkIdTariff, int fkIdRoute, int estimatedMinutes)
    {
        FkIdTariff = fkIdTariff;
        FkIdRoute = fkIdRoute;
        EstimatedMinutes = estimatedMinutes;
    }
}
