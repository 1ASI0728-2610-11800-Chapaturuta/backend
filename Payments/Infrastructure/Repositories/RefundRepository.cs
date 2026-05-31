using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Payments.Infrastructure.Repositories;

public class RefundRepository(AppDbContext context) : BaseRepository<Refund>(context), IRefundRepository
{
    public async Task<IEnumerable<Refund>> FindByPaymentIdAsync(int fkIdPayment)
    {
        return await Context.Set<Refund>()
            .Where(r => r.FkIdPayment == fkIdPayment)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}
