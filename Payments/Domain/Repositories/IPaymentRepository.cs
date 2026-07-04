using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Payments.Domain.Repositories;

public interface IPaymentRepository : IBaseRepository<Payment>
{
    Task<IEnumerable<Payment>> FindByUserIdAsync(int fkIdUser);
    Task<IEnumerable<Payment>> FindByReferenceAsync(string referenceType, int referenceId);
    Task<List<Payment>> FindByReferenceTypeAndReferenceIdsAsync(string referenceType, IEnumerable<int> referenceIds);
}
