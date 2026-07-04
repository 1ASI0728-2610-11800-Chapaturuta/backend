using Frock_backend.Payments.Domain.Model.Aggregates;

namespace Frock_backend.Payments.Domain.Model.Queries;

// Read model pairing a payment received by a driver with the resolved display name of the
// passenger who paid it. PayerName is display-only — never persisted alongside the aggregate.
public record ReceivedPaymentView(Payment Payment, string? PayerName);
