using Frock_backend.Driver.Domain.Model.ValueObjects;

namespace Frock_backend.Driver.Domain.Model.Aggregates;

public class Tariff
{
    public int Id { get; }
    public int FkIdDriver { get; set; }
    public decimal BaseFare { get; set; }
    public decimal PricePerKm { get; set; }
    public decimal PricePerMinute { get; set; }
    public decimal MinFare { get; set; }
    public string Currency { get; set; }
    public WeeklyAvailability WeeklyAvailability { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    protected Tariff()
    {
        Currency = "PEN";
        WeeklyAvailability = new WeeklyAvailability();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public Tariff(
        int fkIdDriver,
        decimal baseFare,
        decimal pricePerKm,
        decimal pricePerMinute,
        decimal minFare,
        string currency,
        WeeklyAvailability weeklyAvailability)
    {
        FkIdDriver = fkIdDriver;
        BaseFare = baseFare;
        PricePerKm = pricePerKm;
        PricePerMinute = pricePerMinute;
        MinFare = minFare;
        Currency = string.IsNullOrWhiteSpace(currency) ? "PEN" : currency;
        WeeklyAvailability = weeklyAvailability ?? new WeeklyAvailability();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePrices(decimal baseFare, decimal pricePerKm, decimal pricePerMinute, decimal minFare)
    {
        BaseFare = baseFare;
        PricePerKm = pricePerKm;
        PricePerMinute = pricePerMinute;
        MinFare = minFare;
    }

    public void UpdateSchedule(WeeklyAvailability weeklyAvailability)
    {
        WeeklyAvailability = weeklyAvailability ?? new WeeklyAvailability();
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
