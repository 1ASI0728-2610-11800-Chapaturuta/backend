using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.Commands;
using Frock_backend.Payments.Domain.Model.ValueObjects;
using Frock_backend.Payments.Domain.Repositories;
using Frock_backend.Payments.Domain.Services;
using Frock_backend.Payments.Infrastructure.Factories;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Payments.Application.Internal.CommandServices;

public class RefundCommandService(
    IRefundRepository refundRepository,
    IPaymentRepository paymentRepository,
    PaymentGatewayFactory gatewayFactory,
    IUnitOfWork unitOfWork) : IRefundCommandService
{
    public async Task<Refund?> Handle(CreateRefundCommand command)
    {
        var payment = await paymentRepository.FindByIdAsync(command.FkIdPayment);
        if (payment == null)
            throw new InvalidOperationException($"Payment {command.FkIdPayment} not found");

        if (payment.Status != PaymentStatus.Completed && payment.Status != PaymentStatus.PartiallyRefunded)
            throw new InvalidOperationException($"Cannot refund payment with status {payment.Status}");

        if (command.Amount <= 0)
            throw new ArgumentException("Refund amount must be greater than zero");

        if (command.Amount > payment.Amount.Amount)
            throw new ArgumentException("Refund amount cannot exceed payment amount");

        var money = new Money(command.Amount, payment.Amount.Currency);
        var refund = new Refund(command.FkIdPayment, money, command.Reason);

        await refundRepository.AddAsync(refund);
        await unitOfWork.CompleteAsync();

        var gateway = gatewayFactory.Resolve(payment.Method);
        var result = await gateway.RefundAsync(payment, command.Amount);

        if (result.Success)
        {
            var isPartial = command.Amount < payment.Amount.Amount;
            payment.MarkRefunded(isPartial);
            paymentRepository.Update(payment);
            await unitOfWork.CompleteAsync();
        }

        return refund;
    }

    public async Task<Refund?> Handle(ConfirmRefundCommand command)
    {
        var refund = await refundRepository.FindByIdAsync(command.RefundId);
        if (refund == null) return null;

        refund.Confirm();
        refundRepository.Update(refund);
        await unitOfWork.CompleteAsync();
        return refund;
    }
}
