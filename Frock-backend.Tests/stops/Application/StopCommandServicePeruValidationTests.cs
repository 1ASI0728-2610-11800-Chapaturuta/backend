using Frock_backend.Driver.Interfaces.ACL;
using Frock_backend.shared.Domain.Repositories;
using Frock_backend.stops.Application.Internal.CommandServices;
using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Model.Commands;
using Frock_backend.stops.Domain.Repositories;
using Frock_backend.Subscriptions.Interfaces.ACL;
using Moq;

namespace Frock_backend.Tests.stops.Application;

/// <summary>
/// Validates that stops can only be created/updated with coordinates inside Peru's bounding box.
/// </summary>
public class StopCommandServicePeruValidationTests
{
    private readonly Mock<IStopRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IDriverContextFacade> _driverFacade = new();
    private readonly Mock<ISubscriptionsContextFacade> _subscriptionsFacade = new();

    private StopCommandService BuildService() =>
        new(_repo.Object, _unitOfWork.Object, _driverFacade.Object, _subscriptionsFacade.Object);

    private static CreateStopCommand Cmd(double? lat, double? lng) =>
        new(
            Name: "Paradero Test",
            GoogleMapsUrl: "",
            ImageUrl: "",
            FkIdDriver: 1,
            Address: "Av. Test 123",
            Reference: "ref",
            FkIdDistrict: 1,
            Latitude: lat,
            Longitude: lng);

    [Theory]
    [InlineData(40.7, -74.0)]    // Nueva York
    [InlineData(0.0, 0.0)]       // Golfo de Guinea
    [InlineData(null, null)]     // sin coordenadas
    [InlineData(-12.05, -40.0)]  // lat de Lima, lng en Brasil (fuera del bbox)
    public async Task CreateStop_Throws_When_Outside_Peru(double? lat, double? lng)
    {
        var service = BuildService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.Handle(Cmd(lat, lng)));

        // No debe consultar ni persistir si la ubicación es inválida.
        _repo.Verify(r => r.AddAsync(It.IsAny<Stop>()), Times.Never);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateStop_Persists_When_Inside_Peru()
    {
        // ARRANGE — Lima
        _repo.Setup(r => r.FindByNameAndFkIdDriverAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((Stop?)null);
        _repo.Setup(r => r.AddAsync(It.IsAny<Stop>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        var service = BuildService();

        // ACT
        var result = await service.Handle(Cmd(-12.05, -77.04));

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(-12.05, result!.Latitude);
        Assert.Equal(-77.04, result.Longitude);
        _repo.Verify(r => r.AddAsync(It.IsAny<Stop>()), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
