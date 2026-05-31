using Frock_backend.Payments.Domain.Model.ValueObjects;
using Frock_backend.Trips.Domain.Model.ValueObjects;

namespace Frock_backend.Trips.Domain.Model.Commands;

public record CreateReservationCommand(
    int FkIdUser,
    int FkIdTrip,
    DocumentType DocumentType,
    string DocumentNumber,
    int Seats,
    PaymentMethod PaymentMethod
);
