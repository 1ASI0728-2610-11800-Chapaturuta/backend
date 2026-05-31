using Frock_backend.Driver.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Driver.Domain.Repositories;

public interface ITariffRepository : IBaseRepository<Tariff>
{
    Task<Tariff?> FindActiveByDriverIdAsync(int driverId);
}
