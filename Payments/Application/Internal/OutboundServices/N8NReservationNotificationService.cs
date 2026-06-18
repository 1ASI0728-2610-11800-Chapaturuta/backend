using System.Net.Http.Json;
using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Services;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using DriverAggregate = Frock_backend.Driver.Domain.Model.Aggregates.Driver;

namespace Frock_backend.Payments.Application.Internal.OutboundServices;

public class N8NReservationNotificationService(
    HttpClient httpClient,
    AppDbContext context,
    IConfiguration configuration,
    ILogger<N8NReservationNotificationService> logger) : IReservationNotificationService
{
    public async Task NotifyReservationConfirmedAsync(Payment payment)
    {
        var webhookUrl = configuration["N8N:ReservationConfirmedWebhookUrl"];
        var demoPassengerPhone = configuration["N8N:DemoPassengerPhone"];

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            logger.LogInformation("Skipping reservation WhatsApp notification because N8N webhook URL is not configured.");
            return;
        }

        if (string.IsNullOrWhiteSpace(demoPassengerPhone))
        {
            logger.LogInformation("Skipping reservation WhatsApp notification because demo passenger phone is not configured.");
            return;
        }

        try
        {
            var reservation = await context.Set<Reservation>()
                .FirstOrDefaultAsync(r => r.Id == payment.ReferenceId);
            if (reservation == null)
            {
                logger.LogWarning("Could not notify N8N: reservation {ReservationId} was not found.", payment.ReferenceId);
                return;
            }

            var trip = await context.Set<Trip>().FirstOrDefaultAsync(t => t.Id == reservation.FkIdTrip);
            if (trip == null)
            {
                logger.LogWarning("Could not notify N8N: trip {TripId} was not found.", reservation.FkIdTrip);
                return;
            }

            var passenger = await context.Set<User>().FirstOrDefaultAsync(u => u.Id == reservation.FkIdUser);
            var origin = await context.Set<Stop>().FirstOrDefaultAsync(s => s.Id == trip.FkIdOriginStop);
            var destination = await context.Set<Stop>().FirstOrDefaultAsync(s => s.Id == trip.FkIdDestinationStop);
            var route = await context.Set<RouteAggregate>().FirstOrDefaultAsync(r => r.Id == trip.FkIdRoute);

            var driverId = trip.FkIdDriver ?? origin?.FkIdDriver;
            DriverAggregate? driver = null;
            if (driverId is > 0)
                driver = await context.Set<DriverAggregate>().FirstOrDefaultAsync(d => d.Id == driverId && !d.IsDeleted);

            var payload = new
            {
                passengerName = passenger?.Username ?? "Pasajero",
                passengerPhone = demoPassengerPhone,
                amountPaid = payment.Amount.Amount,
                currency = payment.Amount.Currency,
                driverName = driver == null ? "Sin conductor" : $"{driver.FirstName} {driver.LastName}".Trim(),
                route = BuildRouteName(trip.FkIdRoute, origin?.Name, destination?.Name),
                seat = reservation.Seats == 1 ? "1 asiento" : $"{reservation.Seats} asientos",
                routeTime = BuildRouteTime(route),
                reservationId = reservation.Id,
                paymentId = payment.Id
            };

            var response = await httpClient.PostAsJsonAsync(webhookUrl, payload);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                logger.LogWarning(
                    "N8N reservation notification failed with status {StatusCode}: {ResponseBody}",
                    response.StatusCode,
                    responseBody);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "N8N reservation notification failed.");
        }
    }

    private static string BuildRouteName(int routeId, string? originName, string? destinationName)
    {
        var origin = string.IsNullOrWhiteSpace(originName) ? "Origen desconocido" : originName;
        var destination = string.IsNullOrWhiteSpace(destinationName) ? "Destino desconocido" : destinationName;
        return $"Ruta {routeId}: {origin} - {destination}";
    }

    private static string BuildRouteTime(RouteAggregate? route)
    {
        if (route?.DurationSeconds is > 0)
        {
            var minutes = Math.Max(1, (int)Math.Ceiling(route.DurationSeconds.Value / 60.0));
            return $"{minutes} min";
        }

        if (route?.Duration > 0)
            return $"{route.Duration} min";

        return "Tiempo no disponible";
    }
}
