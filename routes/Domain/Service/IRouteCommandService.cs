using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Model.Commands;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.routes.Domain.Service
{
    public interface IRouteCommandService
    {
        Task<RouteAggregate?> Handle(CreateFullRouteCommand command);
        Task<RouteAggregate?> Handle(int IdRoute, UpdateRouteCommand command);
        Task Handle(DeleteRouteCommand command);
        Task<RouteAggregate?> ToggleAvailability(int idRoute);
    }
}
