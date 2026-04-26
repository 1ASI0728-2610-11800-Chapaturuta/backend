using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;

using Frock_backend.IAM.Interfaces.REST.Resources;
using Frock_backend.routes.Interface.REST.Resources;
using Frock_backend.stops.Interfaces.REST.Resources;
using Frock_backend.transport_Company.Interfaces.REST.Resources;
using Frock_backend.Trips.Interfaces.REST.Resources;
using Frock_backend.Ratings.Interfaces.REST.Resources;
using Frock_backend.Collections.Interfaces.REST.Resources;

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

            [nameof(UpdateStopResource)] =
                """{"id":1,"name":"Paradero Miraflores","googleMapsUrl":"https://maps.google.com/?q=-12.1191,-77.0310","imageUrl":"https://example.com/stop.jpg","phone":"01-4441234","fkIdCompany":1,"address":"Av. Larco 123","reference":"Frente al parque","fkIdDistrict":15,"latitude":-12.1191,"longitude":-77.0310}""",

            [nameof(UpdateCompanyResource)] =
                """{"id":1,"name":"Empresa Lima Norte S.A.","logoUrl":"https://example.com/logo.jpg","fkIdUser":2}""",

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

            [nameof(CreateDriverProfileResource)] =
                """{"fkIdUser":3,"licenseNumber":"Q56789012","vehiclePlate":"ABC-123","vehicleModel":"Toyota Coaster","vehicleYear":2020,"vehicleCapacity":25}""",

            [nameof(RoutePreviewResource)] =
                """{"coordinates":[{"latitude":-12.0464,"longitude":-77.0428},{"latitude":-12.0500,"longitude":-77.0500}]}""",
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
