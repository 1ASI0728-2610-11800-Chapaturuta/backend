using Frock_backend.Driver.Domain.Model.Aggregates;
using Frock_backend.Driver.Domain.Model.Queries;
using Frock_backend.Driver.Domain.Repositories;
using Frock_backend.Driver.Domain.Services;

namespace Frock_backend.Driver.Application.Internal.QueryServices;

public class TariffQueryService(
    ITariffRepository tariffRepository,
    IRouteDurationRepository routeDurationRepository,
    IDriverRepository driverRepository) : ITariffQueryService
{
    public async Task<Tariff?> Handle(GetTariffByDriverIdQuery query)
    {
        return await tariffRepository.FindActiveByDriverIdAsync(query.FkIdDriver);
    }

    public async Task<RouteDuration?> Handle(GetRouteDurationByDriverAndRouteQuery query)
    {
        var driver = await driverRepository.FindByIdAsync(query.FkIdDriver);
        if (driver == null) return null;

        var tariff = await tariffRepository.FindActiveByDriverIdAsync(query.FkIdDriver);
        if (tariff == null) return null;

        return await routeDurationRepository.FindByTariffAndRouteAsync(tariff.Id, query.FkIdRoute);
    }
}
