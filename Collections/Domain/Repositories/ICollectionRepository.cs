using Frock_backend.Collections.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Collections.Domain.Repositories;

public interface ICollectionRepository : IBaseRepository<Collection>
{
    Task<IEnumerable<Collection>> FindByUserIdAsync(int userId);
    Task<Collection?> FindByIdWithItemsAsync(int collectionId);
}
