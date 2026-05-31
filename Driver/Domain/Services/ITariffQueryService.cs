using Frock_backend.Driver.Domain.Model.Aggregates;
using Frock_backend.Driver.Domain.Model.Queries;

namespace Frock_backend.Driver.Domain.Services;

public interface ITariffQueryService
{
    Task<Tariff?> Handle(GetTariffByDriverIdQuery query);
    Task<RouteDuration?> Handle(GetRouteDurationByDriverAndRouteQuery query);
}
