namespace Frock_backend.Driver.Domain.Model.Commands;

public record UpdateTariffCommand(
    int Id,
    decimal BaseFare,
    decimal PricePerKm,
    decimal PricePerMinute,
    decimal MinFare,
    IEnumerable<DayOfWeek> AvailableDays
);
