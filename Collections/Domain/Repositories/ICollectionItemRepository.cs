using Frock_backend.Collections.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Collections.Domain.Repositories;

public interface ICollectionItemRepository : IBaseRepository<CollectionItem>
{
    Task<CollectionItem?> FindByCollectionAndRouteAsync(int collectionId, int routeId);
    Task<IEnumerable<CollectionItem>> FindByCollectionIdAsync(int collectionId);
}
