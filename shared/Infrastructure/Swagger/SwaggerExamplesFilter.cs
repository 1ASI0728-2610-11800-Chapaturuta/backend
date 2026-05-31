using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;

using Frock_backend.IAM.Interfaces.REST.Resources;
using Frock_backend.routes.Interface.REST.Resources;
using Frock_backend.stops.Interfaces.REST.Resources;
using Frock_backend.Trips.Interfaces.REST.Resources;
using Frock_backend.Ratings.Interfaces.REST.Resources;
using Frock_backend.Collections.Interfaces.REST.Resources;

// New BC resources (wired by F4)
using Frock_backend.Driver.Interfaces.REST.Resources;
using Frock_backend.Subscriptions.Interfaces.REST.Resources;
using Frock_backend.Payments.Interfaces.REST.Resources;

namespace Frock_backend.shared.Infrastructure.Swagger;

/// <summary>
/// IOperationFilter that injects realistic OpenAPI request body examples for known resource types.
/// Registered in Program.cs via options.OperationFilter&lt;SwaggerExamplesFilter&gt;().
/// </summary>
public class SwaggerExamplesFilter : IOperationFilter
{
    private static readonly IReadOnlyDictionary<string, string> ExamplesBySchemaRef =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(SignInResource)] =
                """{"email":"juan.perez@gmail.com","password":"Secure123!"}""",

            [nameof(SignUpResource)] =
                """{"username":"juan_perez","email":"juan.perez@gmail.com","password":"Secure123!","role":0}""",

            [nameof(CreateFullRouteResource)] =
                """{"frequency":15,"price":1.50,"duration":45,"stopsIds":[1,2,3],"schedules":[{"dayOfWeek":"Lunes","startTime":"06:00","endTime":"22:00","enabled":true}]}""",

            [nameof(UpdateRouteResource)] =
                """{"price":1.50,"duration":45,"frequency":15,"stopsIds":[1,2,3],"schedules":[{"startTime":"06:00","endTime":"22:00","dayOfWeek":"Lunes","enabled":true}]}""",

            [nameof(CreateStopResource)] =
                """{"name":"Paradero Miraflores","googleMapsUrl":"https://maps.google.com/?q=-12.1191,-77.0310","imageUrl":"https://example.com/stop.jpg","fkIdDriver":1,"address":"Av. Larco 123","reference":"Frente al parque","fkIdDistrict":15,"latitude":-12.1191,"longitude":-77.0310}""",

            [nameof(UpdateStopResource)] =
                """{"id":1,"name":"Paradero Miraflores","googleMapsUrl":"https://maps.google.com/?q=-12.1191,-77.0310","imageUrl":"https://example.com/stop.jpg","fkIdDriver":1,"address":"Av. Larco 123","reference":"Frente al parque","fkIdDistrict":15,"latitude":-12.1191,"longitude":-77.0310}""",

            [nameof(StopResource)] =
                """{"id":1,"name":"Paradero Miraflores","googleMapsUrl":"https://maps.google.com/?q=-12.1191,-77.0310","imageUrl":"https://example.com/stop.jpg","fkIdDriver":1,"address":"Av. Larco 123","reference":"Frente al parque","fkIdDistrict":15,"latitude":-12.1191,"longitude":-77.0310}""",

            [nameof(CreateTripResource)] =
                """{"fkIdUser":5,"fkIdDriver":3,"fkIdRoute":2,"fkIdOriginStop":4,"fkIdDestinationStop":7,"price":1.50}""",

            [nameof(CreateRatingResource)] =
                """{"fkIdUser":5,"fkIdDriver":3,"fkIdTrip":12,"score":5,"comment":"Excelente conductor, puntual"}""",

            [nameof(CreateCollectionResource)] =
                """{"name":"Mis rutas favoritas","fkIdUser":5}""",

            [nameof(UpdateCollectionResource)] =
                """{"name":"Rutas al trabajo"}""",

            [nameof(UpdateUserProfileResource)] =
                """{"username":"juan_perez","email":"juan.perez@gmail.com"}""",

            [nameof(UpdateUserRoleResource)] =
                """{"role":1}""",

            [nameof(RoutePreviewResource)] =
                """{"coordinates":[{"latitude":-12.0464,"longitude":-77.0428},{"latitude":-12.0500,"longitude":-77.0500}]}""",

            // ── Driver BC ─────────────────────────────────────────────────────
            [nameof(CreateDriverResource)] =
                """{"fkIdUser":3,"firstName":"Juan","lastName":"Perez","documentNumber":"75123456","phone":"987654321","photoUrl":"https://example.com/driver.jpg","licenseNumber":"Q56789012","licenseCategory":"AIIa","vehiclePlate":"ABC-123","vehicleBrand":"Toyota","vehicleModel":"Coaster","vehicleYear":2020,"vehicleCapacity":25,"vehicleType":"Combi"}""",

            [nameof(DriverResource)] =
                """{"id":1,"fkIdUser":3,"firstName":"Juan","lastName":"Perez","documentNumber":"75123456","phone":"987654321","photoUrl":"https://example.com/driver.jpg","licenseNumber":"Q56789012","licenseCategory":"AIIa","vehiclePlate":"ABC-123","vehicleBrand":"Toyota","vehicleModel":"Coaster","vehicleYear":2020,"vehicleCapacity":25,"vehicleType":"Combi","isAvailable":true,"createdAt":"2025-01-15T10:00:00Z","updatedAt":null}""",

            [nameof(UpdateDriverResource)] =
                """{"firstName":"Juan","lastName":"Perez","phone":"987654321","photoUrl":"https://example.com/driver.jpg"}""",

            [nameof(UpdateVehicleResource)] =
                """{"plate":"ABC-123","brand":"Toyota","model":"Coaster","year":2020,"capacity":25,"vehicleType":"Combi"}""",

            [nameof(CreateTariffResource)] =
                """{"fkIdDriver":1,"baseFare":2.50,"pricePerKm":0.80,"pricePerMinute":0.15,"minFare":3.00,"currency":"PEN","availableDays":["Monday","Tuesday","Wednesday","Thursday","Friday"]}""",

            [nameof(TariffResource)] =
                """{"id":1,"fkIdDriver":1,"baseFare":2.50,"pricePerKm":0.80,"pricePerMinute":0.15,"minFare":3.00,"currency":"PEN","availableDays":["Monday","Tuesday","Wednesday","Thursday","Friday"],"isActive":true,"createdAt":"2025-01-15T10:00:00Z"}""",

            [nameof(UpdateTariffResource)] =
                """{"baseFare":2.80,"pricePerKm":0.90,"pricePerMinute":0.20,"minFare":3.50,"availableDays":["Monday","Tuesday","Wednesday","Thursday","Friday","Saturday"]}""",

            [nameof(SetRouteDurationResource)] =
                """{"fkIdRoute":2,"estimatedMinutes":45}""",

            [nameof(RouteDurationResource)] =
                """{"id":1,"fkIdTariff":1,"fkIdRoute":2,"estimatedMinutes":45}""",

            // ── Subscriptions BC ──────────────────────────────────────────────
            [nameof(CreatePlanResource)] =
                """{"name":"Premium","planType":"Premium","targetRole":"Both","price":29.90,"currency":"PEN","billingCycle":"Monthly","benefits":"Uso ilimitado de Discovery con IA","discoveryQuota":null}""",

            [nameof(PlanResource)] =
                """{"id":1,"name":"Premium","planType":"Premium","targetRole":"Both","price":29.90,"currency":"PEN","billingCycle":"Monthly","benefits":"Uso ilimitado de Discovery con IA","discoveryQuota":null,"isActive":true}""",

            [nameof(SubscribeToPlanResource)] =
                """{"fkIdUser":5,"fkIdPlan":2,"autoRenew":true,"paymentMethod":"Yape"}""",

            [nameof(SubscriptionResource)] =
                """{"id":1,"fkIdUser":5,"fkIdPlan":2,"status":"Active","startsAt":"2025-01-15T00:00:00Z","endsAt":"2025-02-15T00:00:00Z","autoRenew":true,"fkIdPayment":10,"discoveryUsageInCycle":3}""",

            // ── Payments BC ───────────────────────────────────────────────────
            [nameof(CreatePaymentResource)] =
                """{"fkIdUser":5,"amount":29.90,"currency":"PEN","method":"Yape","referenceType":"Subscription","referenceId":1}""",

            [nameof(PaymentResource)] =
                """{"id":10,"fkIdUser":5,"amount":29.90,"currency":"PEN","method":"Yape","status":"Confirmed","externalReference":"YAPE-20250115-001","referenceType":"Subscription","referenceId":1,"createdAt":"2025-01-15T10:00:00Z","confirmedAt":"2025-01-15T10:01:00Z"}""",

            [nameof(ConfirmPaymentResource)] =
                """{"externalReference":"YAPE-20250115-001"}""",

            [nameof(CreateRefundResource)] =
                """{"amount":29.90,"reason":"Cliente solicito cancelacion de la suscripcion"}""",

            [nameof(RefundResource)] =
                """{"id":3,"fkIdPayment":10,"amount":29.90,"currency":"PEN","reason":"Cliente solicito cancelacion","status":"Confirmed","createdAt":"2025-01-16T09:00:00Z","confirmedAt":"2025-01-16T09:05:00Z"}""",

            // ── Trips: Reservations ───────────────────────────────────────────
            [nameof(CreateReservationResource)] =
                """{"fkIdUser":5,"fkIdTrip":12,"documentType":"Dni","documentNumber":"75123456","seats":2,"paymentMethod":"Yape"}""",

            [nameof(ReservationResource)] =
                """{"id":1,"fkIdUser":5,"fkIdTrip":12,"documentType":"Dni","documentNumber":"75123456","seats":2,"status":"Confirmed","fkIdPayment":10,"reservedAt":"2025-01-15T10:00:00Z","confirmedAt":"2025-01-15T10:01:00Z"}""",
        };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody is null)
            return;

        foreach (var (mediaType, mediaTypeValue) in operation.RequestBody.Content)
        {
            // Skip multipart/form-data — handled by [FromForm] endpoints
            if (mediaType.Contains("multipart", StringComparison.OrdinalIgnoreCase))
                continue;

            var schemaRef = mediaTypeValue.Schema?.Reference?.Id
                            ?? mediaTypeValue.Schema?.AllOf?.FirstOrDefault()?.Reference?.Id;

            if (schemaRef is null)
                continue;

            if (ExamplesBySchemaRef.TryGetValue(schemaRef, out var json))
            {
                mediaTypeValue.Example = new OpenApiString(json);
            }
        }
    }
}
