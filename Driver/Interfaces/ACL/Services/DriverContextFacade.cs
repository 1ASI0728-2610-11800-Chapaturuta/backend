using Frock_backend.Driver.Domain.Repositories;

namespace Frock_backend.Driver.Interfaces.ACL.Services;

public class DriverContextFacade(
    IDriverRepository driverRepository,
    ITariffRepository tariffRepository,
    IRouteDurationRepository routeDurationRepository) : IDriverContextFacade
{
    /**
     * <summary>
     *     Checks whether a driver exists given its identifier.
     * </summary>
     * <param name="driverId">The driver identifier.</param>
     * <returns>True when the driver exists, false otherwise.</returns>
     */
    public async Task<bool> ExistsDriverAsync(int driverId)
    {
        var driver = await driverRepository.FindByIdAsync(driverId);
        return driver != null && !driver.IsDeleted;
    }

    /**
     * <summary>
     *     Returns an approximate fare for the given driver/route pair using the driver's
     *     active tariff and configured route duration. The approximation is BaseFare + MinFare.
     * </summary>
     * <param name="driverId">The driver identifier.</param>
     * <param name="routeId">The route identifier.</param>
     * <returns>The approximate fare, or null if no active tariff or route duration is configured.</returns>
     */
    public async Task<decimal?> GetEstimatedFareAsync(int driverId, int routeId)
    {
        var tariff = await tariffRepository.FindActiveByDriverIdAsync(driverId);
        if (tariff == null) return null;

        var routeDuration = await routeDurationRepository.FindByTariffAndRouteAsync(tariff.Id, routeId);
        if (routeDuration == null) return null;

        return tariff.BaseFare + tariff.MinFare;
    }

    /**
     * <summary>
     *     Resolves the driver identifier from an IAM user identifier.
     * </summary>
     * <param name="userId">The IAM user identifier.</param>
     * <returns>The driver identifier, or null if no driver matches the user.</returns>
     */
    public async Task<int?> FetchDriverIdByUserIdAsync(int userId)
    {
        var driver = await driverRepository.FindByFkIdUserAsync(userId);
        return driver?.Id;
    }
}
