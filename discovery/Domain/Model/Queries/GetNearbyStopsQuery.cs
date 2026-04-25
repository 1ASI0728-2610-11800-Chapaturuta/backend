namespace Frock_backend.Discovery.Domain.Model.Queries;

public record GetNearbyStopsQuery(double Latitude, double Longitude, double RadiusKm = 2.0);
