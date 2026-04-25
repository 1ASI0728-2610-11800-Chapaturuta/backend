# Chapaturuta Backend API

Backend monolito para la plataforma de transporte **Chapaturuta (Frock)**, desarrollado con ASP.NET Core 9 siguiendo arquitectura DDD (Domain-Driven Design) y patrón CQRS.

## Tech Stack

| Tecnologia | Version |
|---|---|
| .NET / ASP.NET Core | 9.0 |
| MySQL | 8.0 |
| Entity Framework Core | 9.0 |
| JWT Bearer Authentication | 8.12 |
| Serilog | 9.0 |
| MediatR | 12.4 |
| FluentValidation | 11.3 |
| Cloudinary SDK | 1.27 |
| Swagger / Swashbuckle | 9.0 |
| Docker | Multi-stage build |

## Arquitectura

```
├── IAM/                    # Identity & Access Management (Users, Auth, Drivers)
├── Collections/            # Colecciones de rutas favoritas
├── Discovery/              # Busqueda, rutas populares, analytics
├── Notifications/          # Notificaciones de usuario
├── Ratings/                # Calificaciones driver/pasajero
├── routes/                 # Rutas de transporte
├── stops/                  # Paraderos y datos geograficos
├── transport Company/      # Empresas de transporte
├── Trips/                  # Viajes
└── shared/                 # Infrastructure compartida (DbContext, Repositories, UnitOfWork)
```

Cada bounded context sigue la estructura DDD:

```
Context/
├── Domain/
│   ├── Model/
│   │   ├── Aggregates/
│   │   ├── Commands/
│   │   ├── Entities/
│   │   ├── Queries/
│   │   └── ValueObjects/
│   ├── Repositories/
│   └── Services/
├── Application/
│   └── Internal/
│       ├── CommandServices/
│       └── QueryServices/
├── Infrastructure/
│   └── Repositories/
└── Interfaces/
    └── REST/
        ├── Resources/
        ├── Transform/
        └── *Controller.cs
```

## Inicio rapido

### Requisitos

- Docker y Docker Compose

### Ejecutar con Docker

```bash
docker compose up -d --build
```

Esto levanta:
- **MySQL 8.0** en puerto `3307`
- **Backend API** en puerto `5027`

Swagger UI: http://localhost:5027/swagger/index.html

### Ejecutar localmente

Requisitos adicionales: .NET 9 SDK, MySQL 8.0 corriendo en `localhost:3306`

```bash
dotnet restore
dotnet run
```

Swagger UI: http://localhost:5027/swagger/index.html

## API Endpoints

### Authentication (`api/authentication`)

| Metodo | Ruta | Descripcion |
|---|---|---|
| POST | `/api/authentication/sign-up` | Registrar usuario |
| POST | `/api/authentication/sign-in` | Iniciar sesion |

### Users (`api/users`)

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/users` | Listar todos los usuarios |
| GET | `/api/users/{id}` | Obtener usuario por ID |
| GET | `/api/users/email/{email}` | Obtener usuario por email |
| PUT | `/api/users/{id}` | Actualizar perfil |
| PUT | `/api/users/{id}/role` | Cambiar rol (admin) |
| POST | `/api/users/driver-profile` | Crear perfil de conductor |
| GET | `/api/users/driver-profile/{userId}` | Obtener perfil de conductor |

### Companies (`api/companies`)

| Metodo | Ruta | Descripcion |
|---|---|---|
| POST | `/api/companies` | Crear empresa (form-data con logo) |
| GET | `/api/companies` | Listar empresas |
| GET | `/api/companies/{id}` | Obtener empresa por ID |
| GET | `/api/companies/user/{userId}` | Verificar empresa de usuario |
| PUT | `/api/companies/{id}` | Actualizar empresa |
| DELETE | `/api/companies/{id}` | Eliminar empresa |

### Stops (`api/stops`)

| Metodo | Ruta | Descripcion |
|---|---|---|
| POST | `/api/stops` | Crear paradero (form-data con imagen) |
| GET | `/api/stops` | Listar paraderos |
| GET | `/api/stops/{id}` | Obtener paradero por ID |
| GET | `/api/stops/company/{companyId}` | Paraderos por empresa |
| GET | `/api/stops/District/{districtId}` | Paraderos por distrito |
| GET | `/api/stops/district/{districtId}/name/{name}` | Paradero por distrito y nombre |
| GET | `/api/stops/company/{companyId}/name/{name}` | Paradero por empresa y nombre |
| PUT | `/api/stops/{id}` | Actualizar paradero |
| DELETE | `/api/stops/{id}` | Eliminar paradero |

### Geographic (`api/geographic`)

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/geographic/regions` | Listar regiones |
| GET | `/api/geographic/regions/{id}` | Obtener region |
| GET | `/api/geographic/provinces` | Listar provincias |
| GET | `/api/geographic/provinces/{id}` | Obtener provincia |
| GET | `/api/geographic/provinces/region/{regionId}` | Provincias por region |
| GET | `/api/geographic/districts` | Listar distritos |
| GET | `/api/geographic/districts/{id}` | Obtener distrito |
| GET | `/api/geographic/districts/province/{provinceId}` | Distritos por provincia |

### Routes (`api/routes`)

| Metodo | Ruta | Descripcion |
|---|---|---|
| POST | `/api/routes` | Crear ruta |
| GET | `/api/routes` | Listar rutas |
| GET | `/api/routes/{id}` | Obtener ruta por ID |
| GET | `/api/routes/company/{companyId}` | Rutas por empresa |
| GET | `/api/routes/district/{districtId}` | Rutas por distrito |
| PUT | `/api/routes/{id}` | Actualizar ruta |
| DELETE | `/api/routes/{id}` | Eliminar ruta |
| PATCH | `/api/routes/{id}/toggle-availability` | Activar/desactivar ruta |

### Trips (`api/trips`)

| Metodo | Ruta | Descripcion |
|---|---|---|
| POST | `/api/trips` | Crear viaje |
| GET | `/api/trips/{id}` | Obtener viaje por ID |
| GET | `/api/trips/user/{userId}` | Historial de viajes (pasajero) |
| GET | `/api/trips/driver/{driverId}` | Historial de viajes (conductor) |

### Ratings (`api/ratings`)

| Metodo | Ruta | Descripcion |
|---|---|---|
| POST | `/api/ratings` | Crear calificacion |
| GET | `/api/ratings/driver/{driverId}` | Calificaciones de conductor |
| GET | `/api/ratings/driver/{driverId}/summary` | Resumen (promedio + total) |
| GET | `/api/ratings/user/{userId}` | Calificaciones hechas por usuario |

### Collections (`api/collections`)

| Metodo | Ruta | Descripcion |
|---|---|---|
| POST | `/api/collections` | Crear coleccion |
| GET | `/api/collections/user/{userId}` | Colecciones de usuario |
| PUT | `/api/collections/{id}` | Renombrar coleccion |
| DELETE | `/api/collections/{id}` | Eliminar coleccion |
| POST | `/api/collections/{id}/routes/{routeId}` | Agregar ruta a coleccion |
| DELETE | `/api/collections/{id}/routes/{routeId}` | Quitar ruta de coleccion |
| GET | `/api/collections/{id}/routes` | Listar rutas de coleccion |

### Notifications (`api/notifications`)

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/notifications/user/{userId}` | Notificaciones de usuario |
| PUT | `/api/notifications/{id}/read` | Marcar como leida |
| DELETE | `/api/notifications/{id}` | Eliminar notificacion |

### Discovery (`api/discovery`)

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/discovery/search?origin=&destination=&date=` | Buscar rutas por origen/destino |
| GET | `/api/discovery/nearby?lat=&lng=&radius=` | Paraderos cercanos (Haversine) |
| GET | `/api/discovery/popular?limit=10` | Rutas populares por viajes |
| GET | `/api/discovery/analytics/demand?districtId=&period=` | Analytics de demanda |

### Health

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/health` | Health check completo |
| GET | `/health/ready` | Readiness probe |

## Variables de entorno

| Variable | Descripcion | Default |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Connection string MySQL | `server=localhost;user=root;password=root;database=frockdb` |
| `ASPNETCORE_ENVIRONMENT` | Entorno | `Development` |
| `GeoApi__BaseUrl` | URL API geografica externa | `https://django-production-0960.up.railway.app/api/districts/` |

## Base de datos

MySQL 8.0 con naming convention `snake_case`. EF Core aplica migraciones automaticamente al iniciar (`EnsureCreated`).

Tablas principales: `users`, `driver_profiles`, `companies`, `stops`, `routes`, `route_stops`, `schedules`, `trips`, `ratings`, `collections`, `collection_items`, `notifications`, `regions`, `provinces`, `districts`.

## Docker

```yaml
# Puertos expuestos
MySQL:   3307 -> 3306
Backend: 5027 -> 8080
```

Build multi-stage: SDK 9.0 (build) -> ASP.NET 9.0 runtime (produccion).

El backend espera a que MySQL este healthy antes de iniciar. Politica `restart: on-failure` para manejar timing de conexion.
