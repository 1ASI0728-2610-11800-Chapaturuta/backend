using Frock_backend.Trips.Domain.Model.Aggregates;

namespace Frock_backend.Tests.Trips.Domain;

public class TripCapacityTests
{
    private static Trip NewTrip(int availableSeats)
    {
        // fkIdUser, fkIdDriver, fkIdRoute, fkIdOriginStop, fkIdDestinationStop, price, availableSeats
        return new Trip(
            fkIdUser: 1,
            fkIdDriver: 2,
            fkIdRoute: 3,
            fkIdOriginStop: 4,
            fkIdDestinationStop: 5,
            price: 10.0m,
            availableSeats: availableSeats);
    }

    [Fact]
    public void ReserveSeats_Decrements_AvailableSeats()
    {
        // ARRANGE
        var trip = NewTrip(availableSeats: 5);

        // ACT
        trip.ReserveSeats(2);

        // ASSERT
        Assert.Equal(3, trip.AvailableSeats);
    }

    [Fact]
    public void ReserveSeats_Throws_When_Not_Enough_Seats()
    {
        // ARRANGE
        var trip = NewTrip(availableSeats: 2);

        // ACT + ASSERT
        var ex = Assert.Throws<InvalidOperationException>(() => trip.ReserveSeats(3));
        Assert.Equal(2, trip.AvailableSeats); // unchanged on failure
        Assert.Contains("Not enough", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReleaseSeats_Increments_AvailableSeats()
    {
        // ARRANGE
        var trip = NewTrip(availableSeats: 1);

        // ACT
        trip.ReleaseSeats(2);

        // ASSERT
        Assert.Equal(3, trip.AvailableSeats);
    }
}
