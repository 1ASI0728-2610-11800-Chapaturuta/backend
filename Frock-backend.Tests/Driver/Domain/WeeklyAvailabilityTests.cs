using Frock_backend.Driver.Domain.Model.ValueObjects;

namespace Frock_backend.Tests.Driver.Domain;

public class WeeklyAvailabilityTests
{
    [Fact]
    public void IsAvailableOn_Returns_True_For_Enabled_Day()
    {
        // ARRANGE
        var availability = new WeeklyAvailability(new[] { DayOfWeek.Monday, DayOfWeek.Wednesday });

        // ACT
        var result = availability.IsAvailableOn(DayOfWeek.Monday);

        // ASSERT
        Assert.True(result);
    }

    [Fact]
    public void IsAvailableOn_Returns_False_For_Disabled_Day()
    {
        // ARRANGE
        var availability = new WeeklyAvailability(new[] { DayOfWeek.Monday });

        // ACT
        var result = availability.IsAvailableOn(DayOfWeek.Sunday);

        // ASSERT
        Assert.False(result);
    }

    [Fact]
    public void Enable_Then_Disable_Removes_Day()
    {
        // ARRANGE
        var availability = new WeeklyAvailability();

        // ACT
        availability.Enable(DayOfWeek.Friday);
        var enabledResult = availability.IsAvailableOn(DayOfWeek.Friday);
        availability.Disable(DayOfWeek.Friday);
        var disabledResult = availability.IsAvailableOn(DayOfWeek.Friday);

        // ASSERT
        Assert.True(enabledResult);
        Assert.False(disabledResult);
    }

    [Fact]
    public void ToCsv_And_FromCsv_RoundTrip()
    {
        // ARRANGE
        var original = new WeeklyAvailability(new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Wednesday,
            DayOfWeek.Friday
        });

        // ACT
        var csv = original.ToCsv();
        var rebuilt = WeeklyAvailability.FromCsv(csv);

        // ASSERT
        Assert.Equal(original.Days.OrderBy(d => d), rebuilt.Days.OrderBy(d => d));
        Assert.True(rebuilt.IsAvailableOn(DayOfWeek.Monday));
        Assert.True(rebuilt.IsAvailableOn(DayOfWeek.Wednesday));
        Assert.True(rebuilt.IsAvailableOn(DayOfWeek.Friday));
        Assert.False(rebuilt.IsAvailableOn(DayOfWeek.Tuesday));
    }
}
