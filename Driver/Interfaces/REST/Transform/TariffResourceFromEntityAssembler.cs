using Frock_backend.Driver.Domain.Model.Aggregates;
using Frock_backend.Driver.Interfaces.REST.Resources;

namespace Frock_backend.Driver.Interfaces.REST.Transform;

public static class TariffResourceFromEntityAssembler
{
    public static TariffResource ToResourceFromEntity(Tariff entity) =>
        new TariffResource(
            entity.Id,
            entity.FkIdDriver,
            entity.BaseFare,
            entity.PricePerKm,
            entity.PricePerMinute,
            entity.MinFare,
            entity.Currency,
            entity.WeeklyAvailability.Days.OrderBy(d => d).ToList(),
            entity.IsActive,
            entity.CreatedAt);
}
