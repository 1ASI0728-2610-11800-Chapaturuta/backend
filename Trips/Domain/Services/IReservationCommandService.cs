using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Commands;

namespace Frock_backend.Trips.Domain.Services;

public interface IReservationCommandService
{
    Task<Reservation?> Handle(CreateReservationCommand command);
    Task<Reservation?> Handle(ConfirmReservationCommand command);
    Task<Reservation?> Handle(CancelReservationCommand command);
}
