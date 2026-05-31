using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.Commands;

namespace Frock_backend.Payments.Domain.Services;

public interface IRefundCommandService
{
    Task<Refund?> Handle(CreateRefundCommand command);
    Task<Refund?> Handle(ConfirmRefundCommand command);
}
