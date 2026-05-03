using Frock_backend.stops.Domain.Model.DTOs;
using Frock_backend.stops.Domain.Services;
using System.Text.Json;

namespace Frock_backend.stops.Application.External
{
    public class GeoImportService : IGeoImportService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiUrl;
        private readonly ILogger<GeoImportService> _logger;
        private readonly IWebHostEnvironment _env;

        public GeoImportService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeoImportService> logger,
            IWebHostEnvironment env)
        {
            _httpClient = httpClient;
            _apiUrl = configuration["GeoApi:BaseUrl"];
            _logger = logger;
            _env = env;
        }

        public async Task<IEnumerable<GeoResponseDto>> GetGeoFromApi()
        {
            // Try external API first when configured.
            if (!string.IsNullOrWhiteSpace(_apiUrl))
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    var response = await _httpClient.GetAsync(_apiUrl, cts.Token);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync(cts.Token);
                    var geoData = JsonSerializer.Deserialize<IEnumerable<GeoResponseDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (geoData != null && geoData.Any())
                    {
                        _logger.LogInformation("Loaded geographic data from external API ({Count} items).", geoData.Count());
                        return geoData;
                    }
                    _logger.LogWarning("External geo API returned no data; falling back to local snapshot.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "External geo API unavailable; falling back to local OSM snapshot.");
                }
            }

            // Fallback: bundled JSON snapshot extracted from OSM Peru PBF (see backend/scripts/extract-geo.mjs).
            return LoadLocalSnapshot();
        }

        private IEnumerable<GeoResponseDto> LoadLocalSnapshot()
        {
            var path = Path.Combine(_env.ContentRootPath, "stops", "Infrastructure", "Seeding", "geo-data.json");
            if (!File.Exists(path))
            {
                _logger.LogError("Local geo snapshot not found at {Path}", path);
                return Enumerable.Empty<GeoResponseDto>();
            }
            try
            {
                var json = File.ReadAllText(path);
                var items = JsonSerializer.Deserialize<IEnumerable<GeoResponseDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? Enumerable.Empty<GeoResponseDto>();
                _logger.LogInformation("Loaded {Count} geo items from local snapshot.", items.Count());
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse local geo snapshot at {Path}", path);
                return Enumerable.Empty<GeoResponseDto>();
            }
        }
    }
}
