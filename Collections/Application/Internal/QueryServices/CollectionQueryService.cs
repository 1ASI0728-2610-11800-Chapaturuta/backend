using Frock_backend.Collections.Domain.Model.Aggregates;
using Frock_backend.Collections.Domain.Model.Queries;
using Frock_backend.Collections.Domain.Repositories;
using Frock_backend.Collections.Domain.Services;

namespace Frock_backend.Collections.Application.Internal.QueryServices;

public class CollectionQueryService(ICollectionRepository collectionRepository, ICollectionItemRepository collectionItemRepository) : ICollectionQueryService
{
    public async Task<IEnumerable<Collection>> Handle(GetCollectionsByUserIdQuery query)
    {
        return await collectionRepository.FindByUserIdAsync(query.UserId);
    }

    public async Task<IEnumerable<CollectionItem>> Handle(GetCollectionRoutesQuery query)
    {
        return await collectionItemRepository.FindByCollectionIdAsync(query.CollectionId);
    }
}
