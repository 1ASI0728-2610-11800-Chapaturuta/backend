using Frock_backend.Payments.Domain.Model.ValueObjects;
using Frock_backend.Trips.Domain.Model.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Trips.Interfaces.REST.Resources;

public record CreateReservationResource(
    [property: SwaggerSchema("ID del usuario pasajero que realiza la reserva")]
    int FkIdUser,
    [property: SwaggerSchema("ID del viaje (Trip) al que pertenece la reserva")]
    int FkIdTrip,
    [property: SwaggerSchema("Tipo de documento del pasajero. Valores permitidos: Dni")]
    DocumentType DocumentType,
    [property: SwaggerSchema("Número de documento del pasajero (por ejemplo, número de DNI)")]
    string DocumentNumber,
    [property: SwaggerSchema("Cantidad de asientos a reservar. Debe ser mayor que cero")]
    int Seats,
    [property: SwaggerSchema("Método de pago. Valores permitidos: Yape, Plin, Card, Cash")]
    PaymentMethod PaymentMethod
);
