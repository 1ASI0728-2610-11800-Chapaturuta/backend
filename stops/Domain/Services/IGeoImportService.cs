using Frock_backend.stops.Domain.Model.DTOs;

namespace Frock_backend.stops.Domain.Services
{
    public interface IGeoImportService
    {
        Task<IEnumerable<GeoResponseDto>> GetGeoFromApi();
    }
}
