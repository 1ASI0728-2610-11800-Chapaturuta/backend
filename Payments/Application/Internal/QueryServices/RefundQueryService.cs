using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.Queries;
using Frock_backend.Payments.Domain.Repositories;
using Frock_backend.Payments.Domain.Services;

namespace Frock_backend.Payments.Application.Internal.QueryServices;

public class RefundQueryService(IRefundRepository refundRepository) : IRefundQueryService
{
    public async Task<IEnumerable<Refund>> Handle(GetRefundsByPaymentIdQuery query)
    {
        return await refundRepository.FindByPaymentIdAsync(query.FkIdPayment);
    }
}
