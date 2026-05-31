using Frock_backend.Trips.Domain.Model.Commands;
using Frock_backend.Trips.Interfaces.REST.Resources;

namespace Frock_backend.Trips.Interfaces.REST.Transform;

public static class CreateReservationCommandFromResourceAssembler
{
    public static CreateReservationCommand ToCommandFromResource(CreateReservationResource resource) =>
        new CreateReservationCommand(
            resource.FkIdUser,
            resource.FkIdTrip,
            resource.DocumentType,
            resource.DocumentNumber,
            resource.Seats,
            resource.PaymentMethod);
}
