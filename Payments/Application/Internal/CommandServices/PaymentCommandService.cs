using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.Commands;
using Frock_backend.Payments.Domain.Model.ValueObjects;
using Frock_backend.Payments.Domain.Repositories;
using Frock_backend.Payments.Domain.Services;
using Frock_backend.Payments.Infrastructure.Factories;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Payments.Application.Internal.CommandServices;

public class PaymentCommandService(
    IPaymentRepository paymentRepository,
    PaymentGatewayFactory gatewayFactory,
    IUnitOfWork unitOfWork) : IPaymentCommandService
{
    public async Task<Payment?> Handle(CreatePaymentCommand command)
    {
        var money = new Money(command.Amount, string.IsNullOrWhiteSpace(command.Currency) ? "PEN" : command.Currency);
        var payment = new Payment(command.FkIdUser, money, command.Method, command.ReferenceType, command.ReferenceId);

        await paymentRepository.AddAsync(payment);
        await unitOfWork.CompleteAsync();

        var gateway = gatewayFactory.Resolve(command.Method);
        var result = await gateway.InitiateAsync(payment);

        if (result.Success && !string.IsNullOrEmpty(result.ExternalReference))
        {
            payment.ExternalReference = result.ExternalReference;
            paymentRepository.Update(payment);
            await unitOfWork.CompleteAsync();
        }

        return payment;
    }

    public async Task<Payment?> Handle(ConfirmPaymentCommand command)
    {
        var payment = await paymentRepository.FindByIdAsync(command.PaymentId);
        if (payment == null) return null;

        // Idempotency: PayU retries the webhook; only confirm a still-pending payment.
        if (payment.Status != PaymentStatus.Pending) return payment;

        payment.Confirm(command.ExternalReference);
        paymentRepository.Update(payment);
        await unitOfWork.CompleteAsync();
        return payment;
    }

    public async Task<Payment?> Handle(FailPaymentCommand command)
    {
        var payment = await paymentRepository.FindByIdAsync(command.PaymentId);
        if (payment == null) return null;

        payment.Fail();
        paymentRepository.Update(payment);
        await unitOfWork.CompleteAsync();
        return payment;
    }
}
