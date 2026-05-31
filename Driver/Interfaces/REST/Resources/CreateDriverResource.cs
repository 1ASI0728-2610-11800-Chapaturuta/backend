using Frock_backend.Driver.Domain.Model.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Driver.Interfaces.REST.Resources;

public record CreateDriverResource(
    [property: SwaggerSchema("ID del usuario IAM asociado (debe tener rol Driver)")]
    int FkIdUser,
    [property: SwaggerSchema("Nombres del conductor")]
    string FirstName,
    [property: SwaggerSchema("Apellidos del conductor")]
    string LastName,
    [property: SwaggerSchema("Numero de documento (DNI) del conductor")]
    string DocumentNumber,
    [property: SwaggerSchema("Telefono de contacto del conductor")]
    string Phone,
    [property: SwaggerSchema("URL de la foto de perfil del conductor")]
    string PhotoUrl,
    [property: SwaggerSchema("Numero de licencia de conducir")]
    string LicenseNumber,
    [property: SwaggerSchema("Categoria de licencia (AIIa, AIIb, AIIIa, AIIIb, AIIIc)")]
    LicenseCategory LicenseCategory,
    [property: SwaggerSchema("Placa del vehiculo")]
    string VehiclePlate,
    [property: SwaggerSchema("Marca del vehiculo")]
    string VehicleBrand,
    [property: SwaggerSchema("Modelo del vehiculo")]
    string VehicleModel,
    [property: SwaggerSchema("Anio de fabricacion del vehiculo (>= 1980)")]
    int VehicleYear,
    [property: SwaggerSchema("Capacidad de pasajeros del vehiculo (>= 1)")]
    int VehicleCapacity,
    [property: SwaggerSchema("Tipo de vehiculo (Car, Pickup, Combi, Van, Bus, Minivan)")]
    VehicleType VehicleType
);
