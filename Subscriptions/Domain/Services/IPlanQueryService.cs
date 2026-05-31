using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.Queries;

namespace Frock_backend.Subscriptions.Domain.Services;

public interface IPlanQueryService
{
    Task<IEnumerable<Plan>> Handle(GetAllPlansQuery query);
    Task<Plan?> Handle(GetPlanByIdQuery query);
    Task<IEnumerable<Plan>> Handle(GetActivePlansByTargetRoleQuery query);
}
