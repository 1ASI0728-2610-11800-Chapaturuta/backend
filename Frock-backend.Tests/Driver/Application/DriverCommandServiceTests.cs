using Frock_backend.Driver.Application.Internal.CommandServices;
using Frock_backend.Driver.Domain.Model.Commands;
using Frock_backend.Driver.Domain.Model.ValueObjects;
using Frock_backend.Driver.Domain.Repositories;
using Frock_backend.IAM.Interfaces.ACL;
using Frock_backend.shared.Domain.Repositories;
using Moq;
using DriverAggregate = Frock_backend.Driver.Domain.Model.Aggregates.Driver;

namespace Frock_backend.Tests.Driver.Application;

public class DriverCommandServiceTests
{
    private static CreateDriverCommand BuildCreateCommand(int userId = 42) => new CreateDriverCommand(
        FkIdUser: userId,
        FirstName: "Juan",
        LastName: "Perez",
        DocumentNumber: "12345678",
        Phone: "999999999",
        PhotoUrl: "https://example.com/photo.jpg",
        LicenseNumber: "L-001",
        LicenseCategory: LicenseCategory.AIIa,
        VehiclePlate: "ABC-123",
        VehicleBrand: "Toyota",
        VehicleModel: "Hilux",
        VehicleYear: 2020,
        VehicleCapacity: 4,
        VehicleType: VehicleType.Pickup);

    [Fact]
    public async Task CreateDriver_Throws_When_User_Not_Driver_Role()
    {
        // ARRANGE
        var driverRepoMock = new Mock<IDriverRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var iamFacadeMock = new Mock<IIamContextFacade>();
        iamFacadeMock
            .Setup(f => f.FetchUserRoleByIdAsync(42))
            .ReturnsAsync("Traveller");

        var service = new DriverCommandService(driverRepoMock.Object, iamFacadeMock.Object, unitOfWorkMock.Object);
        var command = BuildCreateCommand(userId: 42);

        // ACT / ASSERT
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.Handle(command));
        Assert.Contains("Driver", ex.Message);
        driverRepoMock.Verify(r => r.AddAsync(It.IsAny<DriverAggregate>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateDriver_Persists_When_User_Is_Driver()
    {
        // ARRANGE
        var driverRepoMock = new Mock<IDriverRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var iamFacadeMock = new Mock<IIamContextFacade>();
        iamFacadeMock
            .Setup(f => f.FetchUserRoleByIdAsync(42))
            .ReturnsAsync("Driver");
        driverRepoMock
            .Setup(r => r.AddAsync(It.IsAny<DriverAggregate>()))
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        var service = new DriverCommandService(driverRepoMock.Object, iamFacadeMock.Object, unitOfWorkMock.Object);
        var command = BuildCreateCommand(userId: 42);

        // ACT
        var result = await service.Handle(command);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(42, result!.FkIdUser);
        Assert.Equal("Juan", result.FirstName);
        Assert.Equal("ABC-123", result.Vehicle.Plate);
        driverRepoMock.Verify(r => r.AddAsync(It.IsAny<DriverAggregate>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
