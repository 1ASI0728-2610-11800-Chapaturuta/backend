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
| OSRM | Self-hosted (ghcr.io/project-osrm/osrm-backend) |
| OpenStreetMap Tile Server | Self-hosted (overv/openstreetmap-tile-server) |

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

## Setup automatizado para nuevos colaboradores

El proyecto está organizado para que un compañero solo tenga que hacer **`git clone` + dos comandos Docker**. No hace falta descargar manualmente el PBF, instalar `osmium`, ni ejecutar nada en el host.

### Diagrama de servicios

```
┌──────────────────────── perfil "setup" (correr 1 sola vez) ────────────────┐
│  pbf-downloader  ──>  osrm-preprocess                                      │
│  (curl Geofabrik)     (osrm-extract / partition / customize)               │
│  ./osm-data/peru-260425.osm.pbf      ./osrm-data/peru-260425.osrm*         │
└────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────── perfil por defecto (uso diario) ───────────────────┐
│  mysql           ──>  backend                                              │
│  osrm-backend    ──>  (consumido por backend on-demand)                    │
└────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────── perfil "tiles" (opcional, +6 GB) ──────────────────┐
│  osm-import (tiles-setup, 1 vez) ──>  osm-tile-server (tiles)              │
└────────────────────────────────────────────────────────────────────────────┘
```

### Cómo funciona el setup

1. **`pbf-downloader`** (perfil `setup`) usa `curlimages/curl` para descargar `peru-latest.osm.pbf` desde Geofabrik (~240 MB) y guardarlo como `./osm-data/peru-260425.osm.pbf`. Si el archivo ya existe, no hace nada — es idempotente.
2. **`osrm-preprocess`** (perfil `setup`) depende de que el downloader termine con éxito (`condition: service_completed_successfully`). Ejecuta `osrm-extract` → `osrm-partition` → `osrm-customize` y deja los `.osrm*` en `./osrm-data/`. Esto sí tarda (10–30 min, depende del hardware).
3. **`backend`** ya **no** depende de OSRM en `depends_on`. Si OSRM no está disponible, las features de routing (preview, ETA, geometry de rutas) devuelven errores controlados, pero el resto de endpoints CRUD funcionan. Esto evita que un compañero tenga que esperar 30 min antes de poder ver Swagger.
4. **Datos geográficos** (regiones / provincias / distritos) se cargan al arrancar desde un snapshot OSM embebido en el repo (`stops/Infrastructure/Seeding/geo-data.json`, 1694 distritos). Generado offline con `backend/scripts/extract-geo.mjs` a partir del PBF + GDAL. Si el API externa configurada en `GeoApi:BaseUrl` está disponible, se usa esa; si falla (timeout, 502, etc.), cae automáticamente al snapshot local. **Cero acción manual**.
5. **Tiles del mapa** (`osm-tile-server`) ahora viven en el perfil `tiles` y NO arrancan por defecto, porque el primer import demora 30 min y consume ~6 GB. El frontend tiene fallback a tiles públicos de OSM, así que el mapa funciona aunque el tile server local esté apagado.

### Inicio rápido (compañero nuevo)

```bash
# 1. (una vez) descargar PBF + preprocesar OSRM
cd backend
docker compose --profile setup up

# 2. (siempre) levantar la stack normal
docker compose up -d --build

# 3. (opcional) levantar tile server local para mapas
docker compose --profile tiles-setup up osm-import   # 1 vez, ~30 min
docker compose --profile tiles up -d osm-tile-server # uso normal
```

Verificación rápida:
- Swagger: http://localhost:5027/swagger/index.html
- Health: http://localhost:5027/health
- OSRM (si terminó preprocess): http://localhost:5001/route/v1/driving/-77.0428,-12.0464;-77.0500,-12.0500
- Tiles (si está activo): http://localhost:8088/tile/10/302/486.png

### Re-generar el snapshot geográfico

Si quieres actualizar `stops/Infrastructure/Seeding/geo-data.json` (rara vez — los distritos peruanos cambian poco):

```bash
# 1. asegúrate de tener el PBF
docker compose --profile setup up pbf-downloader

# 2. generar geojsonseq con GDAL (extrae multipolygons admin_level 4/6/8)
docker run --rm -v "$(pwd)/osm-data:/data" ghcr.io/osgeo/gdal:alpine-small-latest \
  ogr2ogr -f GeoJSONSeq /data/admin-poly.geojsonseq /data/peru-260425.osm.pbf multipolygons \
  -where "boundary='administrative' AND admin_level IN ('4','6','8')"

# 3. correr el script Node (genera geo-data.json con jerarquía dep→prov→dist)
cd scripts
npm install
node --max-old-space-size=4096 extract-geo.mjs
```

El script usa `@turf/boolean-point-in-polygon` para inferir la jerarquía cuando OSM no provee tags `is_in:*` y genera UBIGEOs sintéticos compatibles con `GeoResponseDto`.

### Estructura de carpetas relevante

```
backend/
├── osm-data/                                  ← .gitignore (PBF se descarga)
│   ├── peru-260425.osm.pbf                    ← descargado por pbf-downloader
│   └── admin-poly.geojsonseq                  ← intermediate (regenerar geo-data)
├── osrm-data/                                 ← .gitignore (lo crea osrm-preprocess)
├── scripts/
│   └── extract-geo.mjs                        ← regenera geo-data.json
└── stops/Infrastructure/Seeding/
    └── geo-data.json                          ← EN EL REPO (1694 distritos)
```

---

## Inicio rápido (legacy)

### Requisitos

- Docker y Docker Compose

### Ejecutar con Docker

```bash
docker compose --profile setup up   # 1ra vez: descarga PBF + preprocesa OSRM
docker compose up -d --build        # uso normal
```

Esto levanta:
- **MySQL 8.0** en puerto `3307`
- **Backend API** en puerto `5027`
- **OSRM routing** en puerto `5001`
- **OSM Tile Server** (perfil `tiles`, opcional) en puerto `8088`

Swagger UI: http://localhost:5027/swagger/index.html

Verificar OSRM: http://localhost:5001/route/v1/driving/-77.0428,-12.0464;-77.0500,-12.0500

Verificar tiles: http://localhost:8088/tile/10/302/486.png

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
| POST | `/api/routes` | Crear ruta (calcula distancia/duración/geometría vía OSRM) |
| GET | `/api/routes` | Listar rutas |
| GET | `/api/routes/{id}` | Obtener ruta (incluye `distanceMeters`, `durationSeconds`, `geometry`) |
| GET | `/api/routes/company/{companyId}` | Rutas por empresa |
| GET | `/api/routes/district/{districtId}` | Rutas por distrito |
| PUT | `/api/routes/{id}` | Actualizar ruta (recalcula OSRM si cambian paraderos) |
| DELETE | `/api/routes/{id}` | Eliminar ruta |
| PATCH | `/api/routes/{id}/toggle-availability` | Activar/desactivar ruta |
| POST | `/api/routes/preview` | Calcular distancia/duración/geometría sin persistir |
| GET | `/api/routes/{id}/geometry` | Obtener solo la geometría (polyline) de una ruta |
| GET | `/api/routes/{id}/eta?lat=&lng=` | Calcular ETA desde posición actual al destino final |

### Config (`api/config`)

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/config/map` | Configuración del mapa (URL tiles, attribution, zoom, bounding box Perú) |

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
| GET | `/api/discovery/search?origin=&destination=&date=` | Buscar rutas (incluye estimado OSRM de distancia/duración cuando origin+destination coinciden con paraderos) |
| GET | `/api/discovery/nearby?lat=&lng=&radius=&useRoadDistance=false` | Paraderos cercanos; `useRoadDistance=true` ordena por tiempo real de carretera vía OSRM |
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
| `Osrm__BaseUrl` | URL interna del servicio OSRM | `http://localhost:5001` |
| `Osrm__TimeoutSeconds` | Timeout HTTP a OSRM (segundos) | `10` |
| `Osrm__Profile` | Perfil de routing OSRM | `driving` |
| `OsmTiles__PublicUrl` | URL de tiles expuesta al frontend | `http://localhost:8088/tile/{z}/{x}/{y}.png` |

## Base de datos

MySQL 8.0 con naming convention `snake_case`. EF Core aplica migraciones automaticamente al iniciar (`EnsureCreated`).

Tablas principales: `users`, `driver_profiles`, `companies`, `stops`, `routes`, `route_stops`, `schedules`, `trips`, `ratings`, `collections`, `collection_items`, `notifications`, `regions`, `provinces`, `districts`.

## Docker

```yaml
# Puertos expuestos
MySQL:      3307 -> 3306
Backend:    5027 -> 8080
OSRM:       5001 -> 5000
Tile Server: 8088 -> 80
```

Build multi-stage: SDK 9.0 (build) -> ASP.NET 9.0 runtime (produccion).

El backend espera a que MySQL este healthy antes de iniciar. Politica `restart: on-failure` para manejar timing de conexion.
