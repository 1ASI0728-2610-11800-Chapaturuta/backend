using Frock_backend.Driver.Domain.Model.Commands;
using Frock_backend.Driver.Domain.Model.ValueObjects;
using Frock_backend.Driver.Domain.Repositories;
using Frock_backend.Driver.Domain.Services;
using Frock_backend.IAM.Interfaces.ACL;
using Frock_backend.shared.Domain.Repositories;
using DriverAggregate = Frock_backend.Driver.Domain.Model.Aggregates.Driver;

namespace Frock_backend.Driver.Application.Internal.CommandServices;

public class DriverCommandService(
    IDriverRepository driverRepository,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork) : IDriverCommandService
{
    public async Task<DriverAggregate?> Handle(CreateDriverCommand command)
    {
        var role = await iamContextFacade.FetchUserRoleByIdAsync(command.FkIdUser);
        if (!string.Equals(role, "Driver", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("User must have role Driver");

        var vehicle = new Vehicle(
            command.VehiclePlate,
            command.VehicleBrand,
            command.VehicleModel,
            command.VehicleYear,
            command.VehicleCapacity,
            command.VehicleType);

        var driver = new DriverAggregate(
            command.FkIdUser,
            command.FirstName,
            command.LastName,
            command.DocumentNumber,
            command.Phone,
            command.PhotoUrl,
            command.LicenseNumber,
            command.LicenseCategory,
            vehicle);

        await driverRepository.AddAsync(driver);
        await unitOfWork.CompleteAsync();
        return driver;
    }

    public async Task<DriverAggregate?> Handle(UpdateDriverCommand command)
    {
        var driver = await driverRepository.FindByIdAsync(command.Id);
        if (driver == null) return null;

        driver.UpdatePersonalInfo(command.FirstName, command.LastName, command.Phone, command.PhotoUrl);
        driverRepository.Update(driver);
        await unitOfWork.CompleteAsync();
        return driver;
    }

    public async Task<DriverAggregate?> Handle(UpdateVehicleCommand command)
    {
        var driver = await driverRepository.FindByIdAsync(command.DriverId);
        if (driver == null) return null;

        var vehicle = new Vehicle(
            command.Plate,
            command.Brand,
            command.Model,
            command.Year,
            command.Capacity,
            command.VehicleType);
        driver.UpdateVehicle(vehicle);
        driverRepository.Update(driver);
        await unitOfWork.CompleteAsync();
        return driver;
    }

    public async Task<DriverAggregate?> Handle(ToggleAvailabilityCommand command)
    {
        var driver = await driverRepository.FindByIdAsync(command.DriverId);
        if (driver == null) return null;

        driver.ToggleAvailability();
        driverRepository.Update(driver);
        await unitOfWork.CompleteAsync();
        return driver;
    }

    public async Task<DriverAggregate?> Handle(UpdateDriverPhotoCommand command)
    {
        var driver = await driverRepository.FindByIdAsync(command.DriverId);
        if (driver == null) return null;

        // Empty strings are ignored by UpdatePersonalInfo; only PhotoUrl is updated.
        driver.UpdatePersonalInfo(string.Empty, string.Empty, string.Empty, command.PhotoUrl);
        driverRepository.Update(driver);
        await unitOfWork.CompleteAsync();
        return driver;
    }

    public async Task<bool> Handle(DeleteDriverCommand command)
    {
        var driver = await driverRepository.FindByIdAsync(command.Id);
        if (driver == null) return false;

        driver.SoftDelete();
        driverRepository.Update(driver);
        await unitOfWork.CompleteAsync();
        return true;
    }
}
