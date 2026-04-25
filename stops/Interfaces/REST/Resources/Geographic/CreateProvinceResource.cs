namespace Frock_backend.stops.Interfaces.REST.Resources.Geographic
{
    public record CreateProvinceResource(
        int Id, // Unique identifier for the province, e.g. "prov-1"
        string Name, 
        int FkIdRegion // Foreign key to the region this province belongs to
        );
}
