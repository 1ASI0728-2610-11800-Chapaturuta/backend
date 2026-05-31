using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.Commands;

namespace Frock_backend.Payments.Domain.Services;

public interface IPaymentCommandService
{
    Task<Payment?> Handle(CreatePaymentCommand command);
    Task<Payment?> Handle(ConfirmPaymentCommand command);
    Task<Payment?> Handle(FailPaymentCommand command);
}
