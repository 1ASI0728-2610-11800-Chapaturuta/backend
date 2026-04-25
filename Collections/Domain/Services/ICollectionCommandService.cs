using Frock_backend.Collections.Domain.Model.Aggregates;
using Frock_backend.Collections.Domain.Model.Commands;

namespace Frock_backend.Collections.Domain.Services;

public interface ICollectionCommandService
{
    Task<Collection?> Handle(CreateCollectionCommand command);
    Task<Collection?> Handle(UpdateCollectionCommand command);
    Task<Collection?> Handle(DeleteCollectionCommand command);
    Task<CollectionItem?> Handle(AddRouteToCollectionCommand command);
    Task Handle(RemoveRouteFromCollectionCommand command);
}
