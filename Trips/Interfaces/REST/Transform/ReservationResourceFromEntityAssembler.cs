using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Interfaces.REST.Resources;

namespace Frock_backend.Trips.Interfaces.REST.Transform;

public static class ReservationResourceFromEntityAssembler
{
    public static ReservationResource ToResourceFromEntity(Reservation entity) =>
        new ReservationResource(
            entity.Id,
            entity.FkIdUser,
            entity.FkIdTrip,
            entity.DocumentType,
            entity.DocumentNumber,
            entity.Seats,
            entity.Status,
            entity.FkIdPayment,
            entity.ReservedAt,
            entity.ConfirmedAt);
}
