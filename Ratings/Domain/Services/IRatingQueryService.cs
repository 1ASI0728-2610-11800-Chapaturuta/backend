using Frock_backend.Ratings.Domain.Model.Aggregates;
using Frock_backend.Ratings.Domain.Model.Queries;

namespace Frock_backend.Ratings.Domain.Services;

public interface IRatingQueryService
{
    Task<IEnumerable<Rating>> Handle(GetRatingsByDriverIdQuery query);
    Task<IEnumerable<Rating>> Handle(GetRatingsByUserIdQuery query);
    Task<(double Average, int Count)> Handle(GetDriverRatingSummaryQuery query);
}
