using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.IAM.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.IAM.Infrastructure.Persistence.EFC.Repositories;

public class DriverProfileRepository(AppDbContext context) : BaseRepository<DriverProfile>(context), IDriverProfileRepository
{
    public async Task<DriverProfile?> FindByUserIdAsync(int userId)
    {
        return await Context.Set<DriverProfile>().FirstOrDefaultAsync(dp => dp.FkIdUser == userId);
    }
}
