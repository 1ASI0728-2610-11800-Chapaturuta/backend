// Infrastructure/Seeding/GeographicDataSeeder.cs
using Frock_backend.stops.Domain.Model.Commands.Geographic;
using Frock_backend.stops.Domain.Model.Queries.Geographic;
using Frock_backend.stops.Domain.Services;
using Frock_backend.stops.Domain.Services.Geographic;
using Frock_backend.stops.Domain.Model.DTOs;
using Microsoft.Extensions.Logging;

namespace Frock_backend.stops.Infrastructure.Seeding
{
    public class GeographicDataSeeder
    {
        private readonly IGeoImportService _geoImportService;
        private readonly IRegionCommandService _regionCommandService;
        private readonly IRegionQueryService _regionQueryService;
        private readonly IProvinceCommandService _provinceCommandService;
        private readonly IProvinceQueryService _provinceQueryService;
        private readonly IDistrictCommandService _districtCommandService;
        private readonly IDistrictQueryService _districtQueryService;
        private readonly ILogger<GeographicDataSeeder> _logger;

        public GeographicDataSeeder(
            IGeoImportService geoImportService,
            IRegionCommandService regionCommandService,
            IRegionQueryService regionQueryService,
            IProvinceCommandService provinceCommandService,
            IProvinceQueryService provinceQueryService,
            IDistrictCommandService districtCommandService,
            IDistrictQueryService districtQueryService,
            ILogger<GeographicDataSeeder> logger)
        {
            _geoImportService = geoImportService;
            _regionCommandService = regionCommandService;
            _regionQueryService = regionQueryService;
            _provinceCommandService = provinceCommandService;
            _provinceQueryService = provinceQueryService;
            _districtCommandService = districtCommandService;
            _districtQueryService = districtQueryService;
            _logger = logger;
        }

        // Incremental seed: inserts only missing regions/provinces/districts so an
        // already-populated DB gets backfilled (e.g. Callao + districts that were
        // missing from the old OSM snapshot) without truncating tables or breaking
        // Stop -> District foreign keys. UBIGEO ids are stable, so re-running is safe.
        public async Task SeedDataAsync()
        {
            _logger.LogInformation("Iniciando la carga de datos geográficos...");

            // 1) Traer todo (API si está configurada, si no el snapshot local completo)
            IEnumerable<GeoResponseDto> raw = await _geoImportService.GetGeoFromApi();
            var rawList = raw.ToList();
            if (rawList.Count == 0)
            {
                _logger.LogWarning("No hay datos geográficos para cargar.");
                return;
            }

            // 2) Conjuntos de ids ya existentes (evita duplicados; CreateDistrict lanza si existe)
            var existingRegions = (await _regionQueryService.Handle(new GetAllRegionsQuery()))
                .Select(r => r.Id).ToHashSet();
            var existingProvinces = (await _provinceQueryService.Handle(new GetAllProvincesQuery()))
                .Select(p => p.Id).ToHashSet();
            var existingDistricts = (await _districtQueryService.Handle(new GetAllDistrictsQuery()))
                .Select(d => d.Id).ToHashSet();

            // 3) Regiones faltantes
            var regiones = rawList
                .Select(x => new { Id = int.Parse(x.CODIGO.Substring(0, 2)), Name = x.NOMBDEP! })
                .DistinctBy(r => r.Id)
                .Where(r => !existingRegions.Contains(r.Id));
            var addedRegions = 0;
            foreach (var r in regiones)
            {
                await _regionCommandService.Handle(new CreateRegionCommand(r.Id, r.Name));
                addedRegions++;
            }

            // 4) Provincias faltantes
            var provincias = rawList
                .Select(x => new {
                    Id = int.Parse(x.CODIGO.Substring(0, 4)),
                    Name = x.NOMBPROV!,
                    RegionId = int.Parse(x.CODIGO.Substring(0, 2))
                })
                .DistinctBy(p => p.Id)
                .Where(p => !existingProvinces.Contains(p.Id));
            var addedProvinces = 0;
            foreach (var p in provincias)
            {
                await _provinceCommandService.Handle(new CreateProvinceCommand(p.Id, p.Name, p.RegionId));
                addedProvinces++;
            }

            // 5) Distritos faltantes
            var distritos = rawList
                .Select(x => new {
                    Id = int.Parse(x.CODIGO),
                    Name = x.NOMBDIST!,
                    ProvinceId = int.Parse(x.CODIGO.Substring(0, 4))
                })
                .DistinctBy(d => d.Id)
                .Where(d => !existingDistricts.Contains(d.Id));
            var addedDistricts = 0;
            foreach (var d in distritos)
            {
                await _districtCommandService.Handle(new CreateDistrictCommand(d.Id, d.Name, d.ProvinceId));
                addedDistricts++;
            }

            _logger.LogInformation(
                "Carga geográfica completada. Nuevos -> Regiones: {Regions}, Provincias: {Provinces}, Distritos: {Districts}",
                addedRegions, addedProvinces, addedDistricts);
        }
    }
}
