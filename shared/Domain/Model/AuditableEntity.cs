using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

namespace Frock_backend.shared.Domain.Model;

public abstract class AuditableEntity : IEntityWithCreatedUpdatedDate
{
    public DateTimeOffset? CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
}
