using Frock_backend.Subscriptions.Domain.Model.ValueObjects;

namespace Frock_backend.Subscriptions.Domain.Model.Queries;

public record GetActivePlansByTargetRoleQuery(TargetRole Role);
