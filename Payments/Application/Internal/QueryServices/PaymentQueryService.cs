using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.Queries;
using Frock_backend.Payments.Domain.Repositories;
using Frock_backend.Payments.Domain.Services;

namespace Frock_backend.Payments.Application.Internal.QueryServices;

public class PaymentQueryService(IPaymentRepository paymentRepository) : IPaymentQueryService
{
    public async Task<Payment?> Handle(GetPaymentByIdQuery query)
    {
        return await paymentRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<Payment>> Handle(GetPaymentsByUserIdQuery query)
    {
        return await paymentRepository.FindByUserIdAsync(query.FkIdUser);
    }

    public async Task<IEnumerable<Payment>> Handle(GetPaymentsByReferenceQuery query)
    {
        return await paymentRepository.FindByReferenceAsync(query.ReferenceType, query.ReferenceId);
    }
}
