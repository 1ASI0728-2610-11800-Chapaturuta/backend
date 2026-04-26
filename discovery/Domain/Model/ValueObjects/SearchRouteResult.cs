using Frock_backend.routes.Domain.Model.Aggregates;

namespace Frock_backend.Discovery.Domain.Model.ValueObjects;

public record SearchRouteResult(
    RouteAggregate Route,
    double? EstimatedDistanceMeters,
    double? EstimatedDurationSeconds
);
