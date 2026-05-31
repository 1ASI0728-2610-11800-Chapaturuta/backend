using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Queries;
using Frock_backend.Trips.Domain.Repositories;
using Frock_backend.Trips.Domain.Services;

namespace Frock_backend.Trips.Application.Internal.QueryServices;

public class ReservationQueryService(IReservationRepository reservationRepository) : IReservationQueryService
{
    public async Task<Reservation?> Handle(GetReservationByIdQuery query)
    {
        return await reservationRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<Reservation>> Handle(GetReservationsByUserIdQuery query)
    {
        return await reservationRepository.FindByUserIdAsync(query.FkIdUser);
    }

    public async Task<IEnumerable<Reservation>> Handle(GetReservationsByTripIdQuery query)
    {
        return await reservationRepository.FindByTripIdAsync(query.FkIdTrip);
    }

    public async Task<IEnumerable<Reservation>> Handle(GetReservationsByDriverIdQuery query)
    {
        return await reservationRepository.FindByDriverIdAsync(query.FkIdDriver);
    }
}
