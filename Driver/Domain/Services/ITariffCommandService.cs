using Frock_backend.Driver.Domain.Model.Aggregates;
using Frock_backend.Driver.Domain.Model.Commands;

namespace Frock_backend.Driver.Domain.Services;

public interface ITariffCommandService
{
    Task<Tariff?> Handle(CreateTariffCommand command);
    Task<Tariff?> Handle(UpdateTariffCommand command);
    Task<RouteDuration?> Handle(SetRouteDurationCommand command);
}
