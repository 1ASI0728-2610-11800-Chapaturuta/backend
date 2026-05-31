using Frock_backend.Driver.Domain.Model.ValueObjects;

namespace Frock_backend.Driver.Domain.Model.Aggregates;

public class Driver
{
    public int Id { get; }
    public int FkIdUser { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DocumentNumber { get; set; }
    public string Phone { get; set; }
    public string PhotoUrl { get; set; }
    public string LicenseNumber { get; set; }
    public LicenseCategory LicenseCategory { get; set; }
    public Vehicle Vehicle { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    protected Driver()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        DocumentNumber = string.Empty;
        Phone = string.Empty;
        PhotoUrl = string.Empty;
        LicenseNumber = string.Empty;
        LicenseCategory = LicenseCategory.AIIa;
        Vehicle = new Vehicle("PENDING", string.Empty, string.Empty, 1980, 1, VehicleType.Car);
        IsAvailable = true;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
    }

    public Driver(
        int fkIdUser,
        string firstName,
        string lastName,
        string documentNumber,
        string phone,
        string photoUrl,
        string licenseNumber,
        LicenseCategory licenseCategory,
        Vehicle vehicle)
    {
        FkIdUser = fkIdUser;
        FirstName = firstName;
        LastName = lastName;
        DocumentNumber = documentNumber;
        Phone = phone;
        PhotoUrl = photoUrl ?? string.Empty;
        LicenseNumber = licenseNumber;
        LicenseCategory = licenseCategory;
        Vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
        IsAvailable = true;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePersonalInfo(string firstName, string lastName, string phone, string photoUrl)
    {
        if (!string.IsNullOrWhiteSpace(firstName)) FirstName = firstName;
        if (!string.IsNullOrWhiteSpace(lastName)) LastName = lastName;
        if (!string.IsNullOrWhiteSpace(phone)) Phone = phone;
        if (photoUrl != null) PhotoUrl = photoUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateVehicle(Vehicle vehicle)
    {
        Vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleAvailability()
    {
        IsAvailable = !IsAvailable;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        IsAvailable = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
