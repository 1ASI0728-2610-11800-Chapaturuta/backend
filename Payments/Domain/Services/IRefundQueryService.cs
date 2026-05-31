using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.Queries;

namespace Frock_backend.Payments.Domain.Services;

public interface IRefundQueryService
{
    Task<IEnumerable<Refund>> Handle(GetRefundsByPaymentIdQuery query);
}
