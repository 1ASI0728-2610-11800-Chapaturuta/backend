using Frock_backend.Driver.Domain.Model.ValueObjects;

namespace Frock_backend.Tests.Driver.Domain;

public class VehicleTests
{
    [Fact]
    public void Vehicle_Ctor_Throws_When_Plate_Empty()
    {
        // ARRANGE
        const string emptyPlate = "";

        // ACT / ASSERT
        var ex = Assert.Throws<ArgumentException>(() =>
            new Vehicle(emptyPlate, "Toyota", "Hilux", 2020, 4, VehicleType.Pickup));
        Assert.Contains("plate", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Vehicle_Ctor_Throws_When_Capacity_Zero()
    {
        // ARRANGE
        const int invalidCapacity = 0;

        // ACT / ASSERT
        var ex = Assert.Throws<ArgumentException>(() =>
            new Vehicle("ABC-123", "Toyota", "Hilux", 2020, invalidCapacity, VehicleType.Pickup));
        Assert.Contains("capacity", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Vehicle_Ctor_Throws_When_Year_Pre_1980()
    {
        // ARRANGE
        const int invalidYear = 1979;

        // ACT / ASSERT
        var ex = Assert.Throws<ArgumentException>(() =>
            new Vehicle("ABC-123", "Toyota", "Hilux", invalidYear, 4, VehicleType.Pickup));
        Assert.Contains("year", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Vehicle_Ctor_Succeeds_With_Valid_Args()
    {
        // ARRANGE
        const string plate = "ABC-123";
        const string brand = "Toyota";
        const string model = "Hilux";
        const int year = 2020;
        const int capacity = 4;
        const VehicleType type = VehicleType.Pickup;

        // ACT
        var vehicle = new Vehicle(plate, brand, model, year, capacity, type);

        // ASSERT
        Assert.Equal(plate, vehicle.Plate);
        Assert.Equal(brand, vehicle.Brand);
        Assert.Equal(model, vehicle.Model);
        Assert.Equal(year, vehicle.Year);
        Assert.Equal(capacity, vehicle.Capacity);
        Assert.Equal(type, vehicle.Type);
    }
}
