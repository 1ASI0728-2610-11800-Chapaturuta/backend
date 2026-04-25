using Frock_backend.Ratings.Domain.Model.Aggregates;
using Frock_backend.Ratings.Domain.Model.Commands;

namespace Frock_backend.Ratings.Domain.Services;

public interface IRatingCommandService
{
    Task<Rating?> Handle(CreateRatingCommand command);
}
