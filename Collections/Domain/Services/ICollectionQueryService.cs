using Frock_backend.Collections.Domain.Model.Aggregates;
using Frock_backend.Collections.Domain.Model.Queries;

namespace Frock_backend.Collections.Domain.Services;

public interface ICollectionQueryService
{
    Task<IEnumerable<Collection>> Handle(GetCollectionsByUserIdQuery query);
    Task<IEnumerable<CollectionItem>> Handle(GetCollectionRoutesQuery query);
}
