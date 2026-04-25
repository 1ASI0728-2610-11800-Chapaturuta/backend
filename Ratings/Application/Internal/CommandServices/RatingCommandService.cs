using Frock_backend.Ratings.Domain.Model.Aggregates;
using Frock_backend.Ratings.Domain.Model.Commands;
using Frock_backend.Ratings.Domain.Repositories;
using Frock_backend.Ratings.Domain.Services;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Ratings.Application.Internal.CommandServices;

public class RatingCommandService(IRatingRepository ratingRepository, IUnitOfWork unitOfWork) : IRatingCommandService
{
    public async Task<Rating?> Handle(CreateRatingCommand command)
    {
        if (command.Score < 1 || command.Score > 5)
            throw new ArgumentException("Score must be between 1 and 5");

        var rating = new Rating(command.FkIdUser, command.FkIdDriver, command.FkIdTrip, command.Score, command.Comment);

        try
        {
            await ratingRepository.AddAsync(rating);
            await unitOfWork.CompleteAsync();
            return rating;
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while creating rating: {e.Message}");
        }
    }
}
