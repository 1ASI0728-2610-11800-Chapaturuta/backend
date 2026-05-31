using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Payments.Domain.Repositories;

public interface IRefundRepository : IBaseRepository<Refund>
{
    Task<IEnumerable<Refund>> FindByPaymentIdAsync(int fkIdPayment);
}
