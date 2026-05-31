namespace Frock_backend.Driver.Domain.Model.Commands;

public record CreateTariffCommand(
    int FkIdDriver,
    decimal BaseFare,
    decimal PricePerKm,
    decimal PricePerMinute,
    decimal MinFare,
    string Currency,
    IEnumerable<DayOfWeek> AvailableDays
);
