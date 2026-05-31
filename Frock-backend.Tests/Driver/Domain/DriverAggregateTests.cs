using Frock_backend.Driver.Domain.Model.ValueObjects;
using DriverAggregate = Frock_backend.Driver.Domain.Model.Aggregates.Driver;

namespace Frock_backend.Tests.Driver.Domain;

public class DriverAggregateTests
{
    private static Vehicle BuildVehicle(string plate = "ABC-123")
        => new Vehicle(plate, "Toyota", "Hilux", 2020, 4, VehicleType.Pickup);

    private static DriverAggregate BuildDriver(Vehicle? vehicle = null)
        => new DriverAggregate(
            fkIdUser: 1,
            firstName: "Juan",
            lastName: "Perez",
            documentNumber: "12345678",
            phone: "999999999",
            photoUrl: "https://example.com/photo.jpg",
            licenseNumber: "L-001",
            licenseCategory: LicenseCategory.AIIa,
            vehicle: vehicle ?? BuildVehicle());

    [Fact]
    public void Driver_Ctor_Sets_IsAvailable_True_By_Default()
    {
        // ARRANGE / ACT
        var driver = BuildDriver();

        // ASSERT
        Assert.True(driver.IsAvailable);
        Assert.False(driver.IsDeleted);
    }

    [Fact]
    public void ToggleAvailability_Flips_Value()
    {
        // ARRANGE
        var driver = BuildDriver();
        var initial = driver.IsAvailable;

        // ACT
        driver.ToggleAvailability();
        var afterFirst = driver.IsAvailable;
        driver.ToggleAvailability();
        var afterSecond = driver.IsAvailable;

        // ASSERT
        Assert.True(initial);
        Assert.False(afterFirst);
        Assert.True(afterSecond);
    }

    [Fact]
    public void SoftDelete_Sets_IsDeleted_True()
    {
        // ARRANGE
        var driver = BuildDriver();

        // ACT
        driver.SoftDelete();

        // ASSERT
        Assert.True(driver.IsDeleted);
        Assert.False(driver.IsAvailable);
        Assert.NotNull(driver.UpdatedAt);
    }

    [Fact]
    public void UpdateVehicle_Replaces_Vehicle_Instance()
    {
        // ARRANGE
        var originalVehicle = BuildVehicle("OLD-111");
        var driver = BuildDriver(originalVehicle);
        var newVehicle = new Vehicle("NEW-222", "Nissan", "Frontier", 2022, 5, VehicleType.Pickup);

        // ACT
        driver.UpdateVehicle(newVehicle);

        // ASSERT
        Assert.Same(newVehicle, driver.Vehicle);
        Assert.Equal("NEW-222", driver.Vehicle.Plate);
        Assert.Equal("Nissan", driver.Vehicle.Brand);
        Assert.NotNull(driver.UpdatedAt);
    }
}
