namespace Frock_backend.Discovery.Interfaces.REST.Resources;

public record NearbyStopResource(int Id, string Name, string Address, double? Latitude, double? Longitude, int FkIdDriver, int FkIdDistrict);
