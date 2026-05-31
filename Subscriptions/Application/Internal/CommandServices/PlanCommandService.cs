using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.Commands;
using Frock_backend.Subscriptions.Domain.Repositories;
using Frock_backend.Subscriptions.Domain.Services;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Subscriptions.Application.Internal.CommandServices;

public class PlanCommandService(IPlanRepository planRepository, IUnitOfWork unitOfWork) : IPlanCommandService
{
    public async Task<Plan?> Handle(CreatePlanCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ArgumentException("Plan name is required");
        if (command.Price < 0)
            throw new ArgumentException("Plan price cannot be negative");

        var plan = new Plan(
            command.Name,
            command.PlanType,
            command.TargetRole,
            command.Price,
            command.Currency,
            command.BillingCycle,
            command.Benefits,
            command.DiscoveryQuota
        );

        try
        {
            await planRepository.AddAsync(plan);
            await unitOfWork.CompleteAsync();
            return plan;
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while creating plan: {e.Message}");
        }
    }

    public async Task<Plan?> Handle(UpdatePlanCommand command)
    {
        var plan = await planRepository.FindByIdAsync(command.Id);
        if (plan == null) return null;

        try
        {
            plan.UpdatePrice(command.Price);
            plan.Benefits = command.Benefits ?? string.Empty;
            plan.DiscoveryQuota = command.DiscoveryQuota;
            if (command.IsActive) plan.Activate();
            else plan.Deactivate();

            planRepository.Update(plan);
            await unitOfWork.CompleteAsync();
            return plan;
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while updating plan: {e.Message}");
        }
    }
}
