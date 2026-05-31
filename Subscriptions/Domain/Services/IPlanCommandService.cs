using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.Commands;

namespace Frock_backend.Subscriptions.Domain.Services;

public interface IPlanCommandService
{
    Task<Plan?> Handle(CreatePlanCommand command);
    Task<Plan?> Handle(UpdatePlanCommand command);
}
