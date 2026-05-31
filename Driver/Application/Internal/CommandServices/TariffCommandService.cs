using Frock_backend.Driver.Domain.Model.Aggregates;
using Frock_backend.Driver.Domain.Model.Commands;
using Frock_backend.Driver.Domain.Model.ValueObjects;
using Frock_backend.Driver.Domain.Repositories;
using Frock_backend.Driver.Domain.Services;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Driver.Application.Internal.CommandServices;

public class TariffCommandService(
    ITariffRepository tariffRepository,
    IRouteDurationRepository routeDurationRepository,
    IUnitOfWork unitOfWork) : ITariffCommandService
{
    public async Task<Tariff?> Handle(CreateTariffCommand command)
    {
        var availability = new WeeklyAvailability(command.AvailableDays ?? Enumerable.Empty<DayOfWeek>());
        var tariff = new Tariff(
            command.FkIdDriver,
            command.BaseFare,
            command.PricePerKm,
            command.PricePerMinute,
            command.MinFare,
            command.Currency,
            availability);

        await tariffRepository.AddAsync(tariff);
        await unitOfWork.CompleteAsync();
        return tariff;
    }

    public async Task<Tariff?> Handle(UpdateTariffCommand command)
    {
        var tariff = await tariffRepository.FindByIdAsync(command.Id);
        if (tariff == null) return null;

        tariff.UpdatePrices(command.BaseFare, command.PricePerKm, command.PricePerMinute, command.MinFare);
        tariff.UpdateSchedule(new WeeklyAvailability(command.AvailableDays ?? Enumerable.Empty<DayOfWeek>()));
        tariffRepository.Update(tariff);
        await unitOfWork.CompleteAsync();
        return tariff;
    }

    public async Task<RouteDuration?> Handle(SetRouteDurationCommand command)
    {
        var tariff = await tariffRepository.FindByIdAsync(command.FkIdTariff);
        if (tariff == null)
            throw new InvalidOperationException($"Tariff {command.FkIdTariff} not found");

        var existing = await routeDurationRepository.FindByTariffAndRouteAsync(command.FkIdTariff, command.FkIdRoute);
        if (existing != null)
        {
            existing.EstimatedMinutes = command.EstimatedMinutes;
            routeDurationRepository.Update(existing);
            await unitOfWork.CompleteAsync();
            return existing;
        }

        var routeDuration = new RouteDuration(command.FkIdTariff, command.FkIdRoute, command.EstimatedMinutes);
        await routeDurationRepository.AddAsync(routeDuration);
        await unitOfWork.CompleteAsync();
        return routeDuration;
    }
}
