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

El proyecto está organizado para que un compañero solo tenga que hacer **`git clone` + un comando Docker**. Por defecto el routing usa el **servidor demo público de OSRM** (`https://router.project-osrm.org`), así que **no hace falta descargar el PBF ni preprocesar nada**. Self-hostear OSRM en local es opcional (perfil `osrm-local`).

### Diagrama de servicios

```
┌──────────────────────── perfil por defecto (uso diario) ───────────────────┐
│  mysql  ──>  backend  ──>  OSRM demo público (router.project-osrm.org)      │
│  (sin descarga de PBF, sin preprocesado, sin contenedor OSRM local)        │
└────────────────────────────────────────────────────────────────────────────┘

┌──────── perfil "setup" + "osrm-local" (opcional: OSRM self-hosted) ─────────┐
│  pbf-downloader  ──>  osrm-preprocess        ──>  osrm-backend             │
│  (curl Geofabrik)     (extract/partition/customize)   (routing local)      │
│  ./osm-data/*.pbf     ./osrm-data/*.osrm*                                   │
└────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────── perfil "tiles" (opcional, +6 GB) ──────────────────┐
│  osm-import (tiles-setup, 1 vez) ──>  osm-tile-server (tiles)              │
└────────────────────────────────────────────────────────────────────────────┘
```

### Cómo funciona el setup

1. **Routing por defecto = demo público.** El backend apunta a `https://router.project-osrm.org` (keyless). No se descarga PBF ni se levanta OSRM local. Trade-off: fair-use ~1 req/seg y sin SLA → suficiente para dev/demo (ver [OSRM: demo vs self-hosted](#osrm-demo-público-default-vs-self-hosted)).
2. **OSRM self-hosted local (opcional).** Solo si quieres routing sin depender del demo: `pbf-downloader` (perfil `setup`) descarga `peru-latest.osm.pbf` desde Geofabrik (~240 MB); `osrm-preprocess` corre `osrm-extract` → `osrm-partition` → `osrm-customize` (10–30 min); luego `osrm-backend` (perfil `osrm-local`) sirve el routing. Recuerda apuntar `Osrm__BaseUrl` de vuelta a `http://osrm-backend:5000`.
3. **`backend`** **no** depende de OSRM en `depends_on`. Si OSRM (demo o local) no está disponible, las features de routing (preview, ETA, geometry) devuelven errores controlados y el resto de endpoints CRUD funcionan igual.
4. **Datos geográficos** (regiones / provincias / distritos) se cargan al arrancar desde un snapshot OSM embebido en el repo (`stops/Infrastructure/Seeding/geo-data.json`, 1694 distritos). Generado offline con `backend/scripts/extract-geo.mjs` a partir del PBF + GDAL. Si el API externa configurada en `GeoApi:BaseUrl` está disponible, se usa esa; si falla (timeout, 502, etc.), cae automáticamente al snapshot local. **Cero acción manual**.
5. **Tiles del mapa** (`osm-tile-server`) ahora viven en el perfil `tiles` y NO arrancan por defecto, porque el primer import demora 30 min y consume ~6 GB. El frontend tiene fallback a tiles públicos de OSM, así que el mapa funciona aunque el tile server local esté apagado.

### Inicio rápido (compañero nuevo)

```bash
# Caso normal: solo levantar la stack. Routing va al demo público de OSRM.
cd backend
docker compose up -d --build      # mysql + backend. Sin descarga de PBF, sin preprocesado.

# (opcional) tile server local para mapas
docker compose --profile tiles-setup up osm-import   # 1 vez, ~30 min
docker compose --profile tiles up -d osm-tile-server # uso normal

# (opcional) OSRM self-hosted local en vez del demo público
docker compose --profile setup run --rm osrm-preprocess   # 1 vez: descarga PBF + preprocesa (10–30 min)
# luego pon Osrm__BaseUrl: "http://osrm-backend:5000" en el backend y:
docker compose --profile osrm-local up -d --build
```

> **Por defecto el routing usa el demo público** `https://router.project-osrm.org` — tanto en dev como en cloud. No necesitas descargar el PBF. El contenedor `osrm-backend` solo arranca con el perfil `osrm-local`.

Verificación rápida:
- Swagger: http://localhost:5027/swagger/index.html
- Health: http://localhost:5027/health
- OSRM demo: https://router.project-osrm.org/route/v1/driving/-77.0428,-12.0464;-77.0500,-12.0500
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
docker compose up -d --build      # uso normal. Routing vía demo público de OSRM.
```

> **Routing por defecto = demo público** `https://router.project-osrm.org` (dev y cloud). El OSRM local es opcional vía perfil `osrm-local` (ver [OSRM: demo vs self-hosted](#osrm-demo-público-default-vs-self-hosted)).

Esto levanta:
- **MySQL 8.0** en puerto `3307`
- **Backend API** en puerto `5027`
- **OSM Tile Server** (perfil `tiles`, opcional) en puerto `8088`
- (opcional, perfil `osrm-local`) **OSRM routing** en puerto `5001`

Swagger UI: http://localhost:5027/swagger/index.html

Verificar OSRM (demo): https://router.project-osrm.org/route/v1/driving/-77.0428,-12.0464;-77.0500,-12.0500

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
| `Osrm__BaseUrl` | URL del servicio OSRM. Default = demo público `https://router.project-osrm.org` (dev y cloud, sin OSRM local). Para self-host local: `http://osrm-backend:5000` con perfil `osrm-local` | `https://router.project-osrm.org` |
| `Osrm__TimeoutSeconds` | Timeout HTTP a OSRM (segundos) | `10` |
| `Osrm__Profile` | Perfil de routing OSRM | `driving` |
| `OsmTiles__PublicUrl` | URL de tiles expuesta al frontend | `http://localhost:8088/tile/{z}/{x}/{y}.png` |

## OSRM: demo público (default) vs self-hosted

Por defecto **dev y cloud usan el mismo servidor demo público** `https://router.project-osrm.org`, así que nadie necesita descargar el PBF ni preprocesar OSRM. En cloud (Render) además ahorra RAM: OSRM self-hosted cuesta ~0.7–1.5 GB residente + 2–4 GB en el preprocesado.

- Sin API key. Cubre Perú (datos OSM globales). Servicios `route` / `table` / `nearest` habilitados, perfil `driving`.
- **Es demo-grade, no producción real**: fair-use ~1 req/seg, **sin SLA** y la política exige `User-Agent` válido (ya lo envía el cliente `osrm`).
- Si el demo está caído o limita por rate, el routing **degrada con gracia**: `/api/routes/preview` y `/eta` devuelven 502 controlado; `discovery/nearby?useRoadDistance=true` cae a distancia Haversine; el resto de CRUD no se ve afectado.
- Para producción real / sin límites: self-host OSRM en local (perfil `osrm-local`, ver arriba) o en cloud (p. ej. Oracle Always-Free ARM 24 GB), o una API gestionada (Mapbox 100k/mes, OpenRouteService 2k/día).

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
