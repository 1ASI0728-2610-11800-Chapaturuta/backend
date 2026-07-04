using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.Queries;

namespace Frock_backend.Payments.Domain.Services;

public interface IPaymentQueryService
{
    Task<Payment?> Handle(GetPaymentByIdQuery query);
    Task<IEnumerable<Payment>> Handle(GetPaymentsByUserIdQuery query);
    Task<IEnumerable<Payment>> Handle(GetPaymentsByReferenceQuery query);
    Task<IEnumerable<ReceivedPaymentView>> Handle(GetPaymentsReceivedByDriverQuery query);
}
