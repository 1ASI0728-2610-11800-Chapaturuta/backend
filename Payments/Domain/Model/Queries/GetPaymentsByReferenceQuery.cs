namespace Frock_backend.Payments.Domain.Model.Queries;

public record GetPaymentsByReferenceQuery(string ReferenceType, int ReferenceId);
