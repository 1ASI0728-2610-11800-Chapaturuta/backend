namespace Frock_backend.Payments.Domain.Model.Commands;

public record CreateRefundCommand(int FkIdPayment, decimal Amount, string Reason);
