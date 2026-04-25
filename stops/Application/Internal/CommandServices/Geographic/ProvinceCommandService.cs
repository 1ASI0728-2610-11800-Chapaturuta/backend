using Frock_backend.shared.Domain.Repositories;
using Frock_backend.stops.Domain.Model.Aggregates.Geographic;
using Frock_backend.stops.Domain.Model.Commands.Geographic;
using Frock_backend.stops.Domain.Repositories.Geographic;
using Frock_backend.stops.Domain.Services.Geographic;

namespace Frock_backend.stops.Application.Internal.CommandServices.Geographic
{
    public class ProvinceCommandService(IProvinceRepository provinceRepository, IUnitOfWork unitOfWork) : IProvinceCommandService
    {
        public async Task<Province?> Handle(CreateProvinceCommand command)
        {
            var existingProvince =
                await provinceRepository.FindByIdIntAsync(command.Id);
            if (existingProvince != null)
            {
                throw new Exception($"Province already exists with that Id.");
            }
            var newProvince = new Province(command);
            try
            {
                await provinceRepository.AddAsync(newProvince);
                await unitOfWork.CompleteAsync();
                return newProvince;
            }
            catch (Exception e)
            {
                // logger?.LogError(e, "Error creating region with name {RegionName} for locality {LocalityId}.", command.Name, command.FkIdLocality);
                return null; // Signal failure to the controller
            }
        }
    }
}
