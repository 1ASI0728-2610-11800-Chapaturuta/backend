using Frock_backend.Driver.Domain.Model.Aggregates;
using Frock_backend.Driver.Domain.Model.ValueObjects;

namespace Frock_backend.Tests.Driver.Domain;

public class TariffAggregateTests
{
    private static Tariff BuildTariff()
        => new Tariff(
            fkIdDriver: 1,
            baseFare: 5.00m,
            pricePerKm: 1.20m,
            pricePerMinute: 0.30m,
            minFare: 6.00m,
            currency: "PEN",
            weeklyAvailability: new WeeklyAvailability(new[] { DayOfWeek.Monday }));

    [Fact]
    public void UpdatePrices_Updates_All_Price_Fields()
    {
        // ARRANGE
        var tariff = BuildTariff();
        const decimal newBaseFare = 10.50m;
        const decimal newPricePerKm = 2.00m;
        const decimal newPricePerMinute = 0.50m;
        const decimal newMinFare = 12.00m;

        // ACT
        tariff.UpdatePrices(newBaseFare, newPricePerKm, newPricePerMinute, newMinFare);

        // ASSERT
        Assert.Equal(newBaseFare, tariff.BaseFare);
        Assert.Equal(newPricePerKm, tariff.PricePerKm);
        Assert.Equal(newPricePerMinute, tariff.PricePerMinute);
        Assert.Equal(newMinFare, tariff.MinFare);
    }

    [Fact]
    public void UpdateSchedule_Replaces_WeeklyAvailability()
    {
        // ARRANGE
        var tariff = BuildTariff();
        var newSchedule = new WeeklyAvailability(new[]
        {
            DayOfWeek.Tuesday,
            DayOfWeek.Thursday,
            DayOfWeek.Saturday
        });

        // ACT
        tariff.UpdateSchedule(newSchedule);

        // ASSERT
        Assert.Same(newSchedule, tariff.WeeklyAvailability);
        Assert.True(tariff.WeeklyAvailability.IsAvailableOn(DayOfWeek.Tuesday));
        Assert.True(tariff.WeeklyAvailability.IsAvailableOn(DayOfWeek.Thursday));
        Assert.True(tariff.WeeklyAvailability.IsAvailableOn(DayOfWeek.Saturday));
        Assert.False(tariff.WeeklyAvailability.IsAvailableOn(DayOfWeek.Monday));
    }

    [Fact]
    public void Deactivate_Sets_IsActive_False()
    {
        // ARRANGE
        var tariff = BuildTariff();
        Assert.True(tariff.IsActive);

        // ACT
        tariff.Deactivate();

        // ASSERT
        Assert.False(tariff.IsActive);
    }
}
