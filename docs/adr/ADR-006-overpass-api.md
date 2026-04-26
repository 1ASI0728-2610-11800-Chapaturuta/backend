# ADR-006: Overpass API para POIs dinámicos (Fase 2)

**Estado**: Propuesto — Scaffolding preparado, lógica pendiente  
**Fecha**: 2026-04-26  
**Contexto**: Chapaturuta Backend API

---

## Contexto

El producto necesita mostrar Puntos de Interés (POIs) reales del mapa — farmacias, hospitales, grifos, restaurantes — cercanos a paraderos y rutas. Estos datos cambian con el tiempo y son demasiado voluminosos para importarlos y mantenerlos en la BD propia.

En Fase 1 se integró OSRM self-hosted para routing. Fase 2 extiende el stack OSM con Overpass API para consultar POIs dinámicamente.

---

## Decisión

**Self-hosted Overpass API** en producción (imagen `wiktorn/overpass-api`), usando el mismo PBF de Perú ya descargado. Para desarrollo local se puede apuntar al endpoint público `https://overpass-api.de/api/interpreter` mientras no haya recursos para correr otro contenedor.

---

## Alternativas consideradas

| Alternativa | Ventaja | Descarte |
|---|---|---|
| Importar POIs a BD propia | Control total, queries SQL | Datos se desactualizan; carga de importación enorme (10M+ nodos Perú) |
| Google Places API | Datos ricos y actualizados | Costo por llamada; dependencia de proveedor externo; sin datos propios |
| Overpass público siempre | Sin infra extra | Rate-limited (10 req/min); inaceptable en producción |
| **Overpass self-hosted** | Sin límite, datos propios, mismo PBF | Requiere ~8 GB RAM y tiempo de import inicial |

---

## Trade-offs

**A favor**
- Reutiliza el PBF ya descargado (`peru-latest.osm.pbf`)
- Sin costo por llamada
- Latencia predecible dentro de la red Docker
- Datos offline — funciona sin internet

**En contra**
- Import inicial tarda 30–60 min en hardware modesto
- Los datos no se actualizan automáticamente (requiere re-import periódico)
- Consume ~8 GB RAM adicionales durante el import
- El lenguaje Overpass QL tiene curva de aprendizaje

---

## Plan de activación

1. Descomentar el servicio `overpass-api` en `docker-compose.yml`
2. Cambiar `Overpass__BaseUrl` en `appsettings.Development.json` a `http://overpass-api:80/api/interpreter`
3. Implementar `IOverpassPoiService` + `OverpassPoiService` en `discovery/Infrastructure/ExternalServices/`
4. Implementar los handlers de las queries `GetNearbyPoisQuery` y `GetPoisAlongRouteQuery`
5. Reemplazar el `StatusCode(501)` stub en `DiscoveryController` con lógica real
6. Tests de integración contra el contenedor Overpass

---

## Variables de entorno

| Variable | Valor dev local | Valor Docker prod |
|---|---|---|
| `Overpass__BaseUrl` | `https://overpass-api.de/api/interpreter` | `http://overpass-api:80/api/interpreter` |
