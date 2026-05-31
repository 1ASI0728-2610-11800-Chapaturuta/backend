namespace Frock_backend.Driver.Domain.Model.ValueObjects;

public record Vehicle
{
    public string Plate { get; init; }
    public string Brand { get; init; }
    public string Model { get; init; }
    public int Year { get; init; }
    public int Capacity { get; init; }
    public VehicleType Type { get; init; }

    public Vehicle(string Plate, string Brand, string Model, int Year, int Capacity, VehicleType Type)
    {
        if (string.IsNullOrWhiteSpace(Plate))
            throw new ArgumentException("Vehicle plate cannot be empty", nameof(Plate));
        if (Capacity < 1)
            throw new ArgumentException("Vehicle capacity must be at least 1", nameof(Capacity));
        if (Year < 1980)
            throw new ArgumentException("Vehicle year must be 1980 or later", nameof(Year));

        this.Plate = Plate;
        this.Brand = Brand ?? string.Empty;
        this.Model = Model ?? string.Empty;
        this.Year = Year;
        this.Capacity = Capacity;
        this.Type = Type;
    }

    // Parameterless ctor for EF Core materialization
    private Vehicle()
    {
        Plate = string.Empty;
        Brand = string.Empty;
        Model = string.Empty;
        Year = 1980;
        Capacity = 1;
        Type = VehicleType.Car;
    }
}
