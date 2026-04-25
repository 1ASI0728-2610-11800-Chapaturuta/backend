namespace Frock_backend.IAM.Domain.Model.Aggregates;

public class DriverProfile
{
    public int Id { get; }
    public int FkIdUser { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public int VehicleYear { get; set; }
    public int VehicleCapacity { get; set; }
    public bool IsAvailable { get; set; }

    protected DriverProfile() { }

    public DriverProfile(int fkIdUser, string licenseNumber, string vehiclePlate, string vehicleModel, int vehicleYear, int vehicleCapacity)
    {
        FkIdUser = fkIdUser;
        LicenseNumber = licenseNumber;
        VehiclePlate = vehiclePlate;
        VehicleModel = vehicleModel;
        VehicleYear = vehicleYear;
        VehicleCapacity = vehicleCapacity;
        IsAvailable = true;
    }
}
