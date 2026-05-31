using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.Queries;
using Frock_backend.Subscriptions.Domain.Repositories;
using Frock_backend.Subscriptions.Domain.Services;

namespace Frock_backend.Subscriptions.Application.Internal.QueryServices;

public class PlanQueryService(IPlanRepository planRepository) : IPlanQueryService
{
    public async Task<IEnumerable<Plan>> Handle(GetAllPlansQuery query)
    {
        return await planRepository.ListAsync();
    }

    public async Task<Plan?> Handle(GetPlanByIdQuery query)
    {
        return await planRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<Plan>> Handle(GetActivePlansByTargetRoleQuery query)
    {
        return await planRepository.FindActiveByTargetRoleAsync(query.Role);
    }
}
