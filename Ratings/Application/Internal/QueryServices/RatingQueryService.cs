using Frock_backend.Ratings.Domain.Model.Aggregates;
using Frock_backend.Ratings.Domain.Model.Queries;
using Frock_backend.Ratings.Domain.Repositories;
using Frock_backend.Ratings.Domain.Services;

namespace Frock_backend.Ratings.Application.Internal.QueryServices;

public class RatingQueryService(IRatingRepository ratingRepository) : IRatingQueryService
{
    public async Task<IEnumerable<Rating>> Handle(GetRatingsByDriverIdQuery query)
    {
        return await ratingRepository.FindByDriverIdAsync(query.DriverId);
    }

    public async Task<IEnumerable<Rating>> Handle(GetRatingsByUserIdQuery query)
    {
        return await ratingRepository.FindByUserIdAsync(query.UserId);
    }

    public async Task<(double Average, int Count)> Handle(GetDriverRatingSummaryQuery query)
    {
        var ratings = await ratingRepository.FindByDriverIdAsync(query.DriverId);
        var ratingList = ratings.ToList();
        if (!ratingList.Any()) return (0, 0);
        return (ratingList.Average(r => r.Score), ratingList.Count);
    }
}
