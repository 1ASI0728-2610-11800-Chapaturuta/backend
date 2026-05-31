namespace Frock_backend.Driver.Interfaces.REST.Resources;

public record TariffResource(
    int Id,
    int FkIdDriver,
    decimal BaseFare,
    decimal PricePerKm,
    decimal PricePerMinute,
    decimal MinFare,
    string Currency,
    IEnumerable<DayOfWeek> AvailableDays,
    bool IsActive,
    DateTime CreatedAt
);
