using Frock_backend.Driver.Application.Internal.CommandServices;
using Frock_backend.Driver.Domain.Model.Aggregates;
using Frock_backend.Driver.Domain.Model.Commands;
using Frock_backend.Driver.Domain.Model.ValueObjects;
using Frock_backend.Driver.Domain.Repositories;
using Frock_backend.shared.Domain.Repositories;
using Moq;

namespace Frock_backend.Tests.Driver.Application;

public class TariffCommandServiceTests
{
    [Fact]
    public async Task CreateTariff_Persists_With_WeeklyAvailability_From_Days()
    {
        // ARRANGE
        var tariffRepoMock = new Mock<ITariffRepository>();
        var routeDurationRepoMock = new Mock<IRouteDurationRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        Tariff? captured = null;
        tariffRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Tariff>()))
            .Callback<Tariff>(t => captured = t)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        var service = new TariffCommandService(
            tariffRepoMock.Object,
            routeDurationRepoMock.Object,
            unitOfWorkMock.Object);

        var days = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
        var command = new CreateTariffCommand(
            FkIdDriver: 7,
            BaseFare: 5m,
            PricePerKm: 1.2m,
            PricePerMinute: 0.3m,
            MinFare: 6m,
            Currency: "PEN",
            AvailableDays: days);

        // ACT
        var result = await service.Handle(command);

        // ASSERT
        Assert.NotNull(result);
        Assert.NotNull(captured);
        Assert.Equal(7, captured!.FkIdDriver);
        Assert.True(captured.WeeklyAvailability.IsAvailableOn(DayOfWeek.Monday));
        Assert.True(captured.WeeklyAvailability.IsAvailableOn(DayOfWeek.Wednesday));
        Assert.True(captured.WeeklyAvailability.IsAvailableOn(DayOfWeek.Friday));
        Assert.False(captured.WeeklyAvailability.IsAvailableOn(DayOfWeek.Tuesday));
        Assert.Equal(3, captured.WeeklyAvailability.Days.Count);
        tariffRepoMock.Verify(r => r.AddAsync(It.IsAny<Tariff>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task SetRouteDuration_Adds_New_Or_Updates_Existing()
    {
        // ARRANGE
        var tariffRepoMock = new Mock<ITariffRepository>();
        var routeDurationRepoMock = new Mock<IRouteDurationRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        const int tariffId = 1;
        const int routeId = 10;
        const int estimatedMinutes = 25;

        var existingTariff = new Tariff(
            fkIdDriver: 7,
            baseFare: 5m,
            pricePerKm: 1.2m,
            pricePerMinute: 0.3m,
            minFare: 6m,
            currency: "PEN",
            weeklyAvailability: new WeeklyAvailability());

        tariffRepoMock
            .Setup(r => r.FindByIdAsync(tariffId))
            .ReturnsAsync(existingTariff);
        routeDurationRepoMock
            .Setup(r => r.FindByTariffAndRouteAsync(tariffId, routeId))
            .ReturnsAsync((RouteDuration?)null);

        RouteDuration? captured = null;
        routeDurationRepoMock
            .Setup(r => r.AddAsync(It.IsAny<RouteDuration>()))
            .Callback<RouteDuration>(rd => captured = rd)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        var service = new TariffCommandService(
            tariffRepoMock.Object,
            routeDurationRepoMock.Object,
            unitOfWorkMock.Object);

        var command = new SetRouteDurationCommand(
            FkIdTariff: tariffId,
            FkIdRoute: routeId,
            EstimatedMinutes: estimatedMinutes);

        // ACT
        var result = await service.Handle(command);

        // ASSERT
        Assert.NotNull(result);
        Assert.NotNull(captured);
        Assert.Equal(tariffId, captured!.FkIdTariff);
        Assert.Equal(routeId, captured.FkIdRoute);
        Assert.Equal(estimatedMinutes, captured.EstimatedMinutes);
        routeDurationRepoMock.Verify(r => r.AddAsync(It.IsAny<RouteDuration>()), Times.Once);
        routeDurationRepoMock.Verify(r => r.Update(It.IsAny<RouteDuration>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
