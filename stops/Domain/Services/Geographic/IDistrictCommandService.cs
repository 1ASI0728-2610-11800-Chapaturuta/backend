using Frock_backend.stops.Domain.Model.Commands.Geographic;
using Frock_backend.stops.Domain.Model.Aggregates.Geographic;

namespace Frock_backend.stops.Domain.Services.Geographic
{
    public interface IDistrictCommandService
    {
        Task<District?> Handle(CreateDistrictCommand command);
    }
}
