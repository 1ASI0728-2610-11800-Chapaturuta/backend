using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Queries;

namespace Frock_backend.Trips.Domain.Services;

public interface IReservationQueryService
{
    Task<Reservation?> Handle(GetReservationByIdQuery query);
    Task<IEnumerable<Reservation>> Handle(GetReservationsByUserIdQuery query);
    Task<IEnumerable<Reservation>> Handle(GetReservationsByTripIdQuery query);
    Task<IEnumerable<Reservation>> Handle(GetReservationsByDriverIdQuery query);
}
