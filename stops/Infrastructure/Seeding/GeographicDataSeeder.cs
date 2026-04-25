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
        private readonly IDistrictCommandService _districtCommandService;
        private readonly ILogger<GeographicDataSeeder> _logger;

        public GeographicDataSeeder(
            IGeoImportService geoImportService,
            IRegionCommandService regionCommandService,
            IRegionQueryService regionQueryService,
            IProvinceCommandService provinceCommandService,
            IDistrictCommandService districtCommandService,
            ILogger<GeographicDataSeeder> logger)
        {
            _geoImportService = geoImportService;
            _regionCommandService = regionCommandService;
            _regionQueryService = regionQueryService;
            _provinceCommandService = provinceCommandService;
            _districtCommandService = districtCommandService;
            _logger = logger;
        }

        public async Task SeedDataAsync()
        {
            _logger.LogInformation("Iniciando la carga de datos geográficos...");

            if ((await _regionQueryService.Handle(new GetAllRegionsQuery())).Any())
            {
                _logger.LogInformation("Los datos geográficos ya están cargados.");
                return;
            }

            // 1) Traer todo desde la API
            IEnumerable<GeoResponseDto> raw = await _geoImportService.GetGeoFromApi();

            // 2) Cargar regiones
            var regiones = raw
                .Select(x => new {
                    Id = int.Parse(x.CODIGO.Substring(0, 2)),
                    Name = x.NOMBDEP!
                })
                .DistinctBy(r => r.Id);

            foreach (var r in regiones)
                await _regionCommandService.Handle(new CreateRegionCommand(r.Id, r.Name));

            // 3) Cargar provincias
            var provincias = raw
                .Select(x => new {
                    Id = int.Parse(x.CODIGO.Substring(0, 4)),
                    Name = x.NOMBPROV!,
                    RegionId = int.Parse(x.CODIGO.Substring(0, 2))
                })
                .DistinctBy(p => p.Id);

            foreach (var p in provincias)
                await _provinceCommandService.Handle(new CreateProvinceCommand(p.Id, p.Name, p.RegionId));

            // 4) Cargar distritos
            var distritos = raw.Select(x => new {
                Id = int.Parse(x.CODIGO),
                Name = x.NOMBDIST!,
                ProvinceId = int.Parse(x.CODIGO.Substring(0, 4))
            });

            foreach (var d in distritos)
                await _districtCommandService.Handle(new CreateDistrictCommand(d.Id, d.Name, d.ProvinceId));

            _logger.LogInformation("Carga de datos geográficos completada con éxito.");
        }
    }
}
