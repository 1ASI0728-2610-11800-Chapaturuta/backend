using Frock_backend.Driver.Domain.Model.Commands;
using Frock_backend.Driver.Interfaces.REST.Resources;

namespace Frock_backend.Driver.Interfaces.REST.Transform;

public static class CreateTariffCommandFromResourceAssembler
{
    public static CreateTariffCommand ToCommandFromResource(CreateTariffResource resource) =>
        new CreateTariffCommand(
            resource.FkIdDriver,
            resource.BaseFare,
            resource.PricePerKm,
            resource.PricePerMinute,
            resource.MinFare,
            string.IsNullOrWhiteSpace(resource.Currency) ? "PEN" : resource.Currency,
            resource.AvailableDays ?? Enumerable.Empty<DayOfWeek>());
}
