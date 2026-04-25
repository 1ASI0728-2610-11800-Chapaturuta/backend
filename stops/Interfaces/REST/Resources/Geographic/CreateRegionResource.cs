namespace Frock_backend.stops.Interfaces.REST.Resources.Geographic
{
    public record CreateRegionResource(
        int Id, // The unique identifier for the region, e.g. reg-1
        string Name // The name of the region
        );
}
