using Frock_backend.Trips.Domain.Model.ValueObjects;

namespace Frock_backend.Trips.Interfaces.REST.Resources;

public record ReservationResource(
    int Id,
    int FkIdUser,
    int FkIdTrip,
    DocumentType DocumentType,
    string DocumentNumber,
    int Seats,
    ReservationStatus Status,
    int? FkIdPayment,
    DateTime ReservedAt,
    DateTime? ConfirmedAt
);
