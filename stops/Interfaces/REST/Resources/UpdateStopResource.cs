namespace Frock_backend.stops.Interfaces.REST.Resources
{
    public record UpdateStopResource(
        int Id, // The ID of the stop to update
        string Name, // The name of the stop
        string GoogleMapsUrl, // The URL to the stop's location on Google Maps, por mientras en null
        string ImageUrl, // The URL to the stop's image
        string Phone, // The phone number associated with the stop
        int FkIdCompany, // This is a foreign key to a Company entity
        string Address, // The address of the stop
        string Reference, // A reference or additional information about the stop
        int FkIdDistrict, // This is a foreign key to a District entity
        double? Latitude = null,
        double? Longitude = null
        );
}