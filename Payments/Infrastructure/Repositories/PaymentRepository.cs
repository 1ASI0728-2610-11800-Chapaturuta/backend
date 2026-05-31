using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Payments.Infrastructure.Repositories;

public class PaymentRepository(AppDbContext context) : BaseRepository<Payment>(context), IPaymentRepository
{
    public async Task<IEnumerable<Payment>> FindByUserIdAsync(int fkIdUser)
    {
        return await Context.Set<Payment>()
            .Where(p => p.FkIdUser == fkIdUser)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> FindByReferenceAsync(string referenceType, int referenceId)
    {
        return await Context.Set<Payment>()
            .Where(p => p.ReferenceType == referenceType && p.ReferenceId == referenceId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
