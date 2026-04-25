using Frock_backend.shared.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;

namespace Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories
{
    public class UnitOfWork(AppDbContext context) : IUnitOfWork
    {
        public async Task CompleteAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
