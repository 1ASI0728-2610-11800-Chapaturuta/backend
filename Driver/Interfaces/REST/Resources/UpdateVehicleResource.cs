using Frock_backend.Driver.Domain.Model.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Driver.Interfaces.REST.Resources;

public record UpdateVehicleResource(
    [property: SwaggerSchema("Placa del vehiculo")]
    string Plate,
    [property: SwaggerSchema("Marca del vehiculo")]
    string Brand,
    [property: SwaggerSchema("Modelo del vehiculo")]
    string Model,
    [property: SwaggerSchema("Anio de fabricacion del vehiculo (>= 1980)")]
    int Year,
    [property: SwaggerSchema("Capacidad de pasajeros (>= 1)")]
    int Capacity,
    [property: SwaggerSchema("Tipo de vehiculo (Car, Pickup, Combi, Van, Bus, Minivan)")]
    VehicleType VehicleType
);
