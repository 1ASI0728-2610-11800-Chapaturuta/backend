using Frock_backend.Trips.Domain.Model.Commands;
using Frock_backend.Trips.Domain.Services;

namespace Frock_backend.Trips.Interfaces.ACL.Services;

public class TripsContextFacade(IReservationCommandService reservationCommandService) : ITripsContextFacade
{
    public async Task ConfirmReservationAsync(int reservationId)
    {
        await reservationCommandService.Handle(new ConfirmReservationCommand(reservationId));
    }
}
